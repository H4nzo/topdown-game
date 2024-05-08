using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

//FINITE STATE MACHINE
public enum EnemyState
{
    Idle,
    Patrol,
    Alert,
    Chase,
    SoundDetected,
    ShootAtSight
}

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour, IHear
{
    #region Variables

    public EnemyState enemyState;
    public GameObject fovMesh;

    [Header("Minimap Features")]
    public SpriteRenderer minimapIcon;
    public Color minimapCurrentColor;
    public Color aliveColor;
    public Color alertColor;
    public Color deathColor;

    [Header("Icon Features")]
    public GameObject alertIcon, chaseTargetIcon, soundIcon;

    [Header("Animation Features")]
    private const string BLENDSTATE = "Speed";
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    [SerializeField] private float idleTimer = 0f;
    private bool isIdling = false;

    [SerializeField] private float alertDuration = 3f; // Adjust as needed
    [SerializeField] private float alertTimer = 0f;
    [SerializeField] private bool isAlerting = false;

    public Waypoint[] waypoints;
    [SerializeField] private Waypoint currentWaypoint;
    [SerializeField] private int currentWaypointIndex = 0;

    [Header("Alert / Detection Features")]
    private Transform target;
    private bool isChasing = false;
    private bool detectPlayer = false;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed = 4.1f;

    private Vector3 lastKnownPosition;
    [SerializeField] private float chaseDistanceThreshold = 10f;
    private float distanceCovered = 0f;

    public float noiseResponseDistance = 10f;

    public bool heardSomething = false;
    public bool isDead;


    #endregion


    public bool isFiring = false;
    public float fireRate = 15f;
    private float nextTimeToFire = 0f;
    private Weapon weapon;

    public static List<NPCController> allNPCs = new List<NPCController>();


    // Start is called before the first frame update
    void Start()
    {
        minimapCurrentColor = minimapIcon.color;

        minimapIcon.color = aliveColor;

        weapon = GetComponent<Weapon>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        patrolSpeed = navMeshAgent.speed;

        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (waypoints.Length > 0)
        {
            SetNextWaypointIndex();
            PatrolToDestination();
        }
        else
        {
            Debug.LogError("No waypoints assigned to NPCController script on " + gameObject.name);
        }

        allNPCs = FindObjectsOfType<NPCController>().Where(npc => npc != this).ToList();


    }

    // Update is called once per frame
    void Update()
    {

        // Check if the NPC is in the SoundDetected state
        if (enemyState != EnemyState.SoundDetected)
        {
            #region Locomotion
            if (!isIdling && !currentWaypoint.IsOccupied())
            {
                currentWaypoint.Occupy(gameObject);
            }
            else if (!isIdling && currentWaypoint.IsOccupied() && currentWaypoint.GetVisitingNPC() != gameObject)
            {
                // Find another unoccupied waypoint
                FindUnoccupiedWaypoint();
            }

            if (navMeshAgent.enabled && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!isIdling)
                {
                    animator.SetFloat(BLENDSTATE, 0f);
                    isIdling = true;
                    idleTimer = 0f;
                }
                else
                {
                    idleTimer += Time.deltaTime;
                    if (idleTimer >= currentWaypoint.idleDuration)
                    {
                        isIdling = false;
                        // Release the current waypoint
                        currentWaypoint.Release();
                        SetNextWaypointIndex();
                        SelectedState();
                    }
                }
            }
            #endregion

            // Check for player detection and initiate alerting if detected
            if (detectPlayer && !isChasing && enemyState != EnemyState.Alert && enemyState != EnemyState.ShootAtSight)
            {
                StartAlerting();
            }

            // Handle alerting behavior
            if (isAlerting)
            {
                alertTimer += Time.deltaTime;
                if (alertTimer >= alertDuration)
                {
                    alertTimer = 0f;
                    isAlerting = false;
                    isFiring = true;
                    // StartChasing();

                }
            }

            if (isFiring == true && Time.time >= nextTimeToFire)
            {
                alertTimer = 0f;
                isAlerting = false;
                nextTimeToFire = Time.time + 1f / fireRate;
                ShootAtTarget();
            }
        }

        // Only execute chasing behavior if not in SoundDetected state
        if (enemyState != EnemyState.SoundDetected)
        {
            if (isChasing)
            {
                ChaseTarget();
                // Calculate distance covered during chase
                distanceCovered += navMeshAgent.velocity.magnitude * Time.deltaTime;
                // Check if the NPC should return to patrol state
                if (distanceCovered >= chaseDistanceThreshold)
                {
                    distanceCovered = 0;
                    ReturnToPatrol();
                }
            }
            else if (isFiring && Vector3.Distance(transform.position, target.position) > noiseResponseDistance)
            {
                isFiring = false;
                weapon.StopShooting();
                StartChasing();
            }
        }



    }


    void ReturnToPatrol()
    {
        minimapIcon.color = aliveColor;

        minimapCurrentColor = minimapIcon.color;

        Debug.Log("Returned Patrol by " + name);
        enemyState = EnemyState.Patrol;
        isChasing = false;


        alertIcon.SetActive(false);
        chaseTargetIcon.SetActive(false);

        navMeshAgent.speed = patrolSpeed;

        Debug.Log("Returning to patrol");

        // Calculate the index of the previous waypoint
        int previousWaypointIndex = currentWaypointIndex - 1;
        if (previousWaypointIndex < 0)
        {
            previousWaypointIndex = waypoints.Length - 1;
        }

        // Set the current waypoint to the previous waypoint
        currentWaypointIndex = previousWaypointIndex;
        SetNextWaypointIndex();
        PatrolToDestination();
    }

    void StartAlerting()
    {
        minimapIcon.color = alertColor;
        minimapCurrentColor = minimapIcon.color;


        enemyState = EnemyState.Alert;
        isAlerting = true;
        animator.SetFloat(BLENDSTATE, 0f);
        navMeshAgent.isStopped = true;
        alertIcon.SetActive(true);
        Debug.Log("Alerting");
    }

    void StartChasing()
    {
        minimapIcon.color = alertColor;
        minimapCurrentColor = minimapIcon.color;

        enemyState = EnemyState.Chase;
        isChasing = true;

        Debug.Log("Chasing");
    }

    void ChaseTarget()
    {
        minimapIcon.color = alertColor;
        minimapCurrentColor = minimapIcon.color;
        
        navMeshAgent.SetDestination(target.position);
        navMeshAgent.isStopped = false;

        alertIcon.SetActive(false);
        chaseTargetIcon.SetActive(true);
        navMeshAgent.speed = chaseSpeed;
        animator.SetFloat(BLENDSTATE, 0.67f);
    }

    private void SelectedState()
    {
        if (enemyState == EnemyState.Idle)
        {
            animator.SetFloat(BLENDSTATE, 0f);
        }
        else
        {
            PatrolToDestination();
        }
    }

    void SetNextWaypointIndex()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Length);
    }

    public void PatrolToDestination()
    {

        currentWaypoint = waypoints[currentWaypointIndex];
        navMeshAgent.SetDestination(currentWaypoint.transform.position);
        navMeshAgent.speed = patrolSpeed;
        animator.SetFloat(BLENDSTATE, .34f);

    }

    void FindUnoccupiedWaypoint()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!waypoints[i].IsOccupied())
            {
                currentWaypoint.Release();
                currentWaypointIndex = i;
                PatrolToDestination();
                return;
            }
        }
    }

    public void DetectedPlayer(bool _detectPlayer)
    {
        detectPlayer = _detectPlayer;
        if (detectPlayer)
        {
            Debug.Log(name + " Detected Player");
            //minimapIcon.color = alertColor;

            minimapCurrentColor = minimapIcon.color;
        }
        else
        {
            //Debug.Log(gameObject.name + " did not Detect Player");
        }
    }


    void ShootAtTarget()
    {
        enemyState = EnemyState.ShootAtSight;
        animator.SetFloat(BLENDSTATE, 1f);
        weapon.Shoot(target);
        weapon.AlignWithEnemy(target);
        if (target.GetComponent<PlayerHealth>().isDead == true)
        {
            target.gameObject.layer = LayerMask.NameToLayer("Default");
            weapon.StopShooting();
        }

    }
    // Implement RespondToSound method from IHear interface
    public void RespondToSound(Sound sound)
    {

        // Check if the NPC is currently being attacked
        if (sound.soundType == Sound.SoundType.Interesting)
        {
            Debug.Log(name + " has detected sound");
            heardSomething = true;
            enemyState = EnemyState.SoundDetected; // Switch to SoundDetected state
            Debug.Log("Switched to Sound Detection state");
            // Reset waypoint and state if currently idling

            MoveToSound(sound.pos);

        }

    }

    private Coroutine moveToSoundCoroutine;

    private void AssignPatrolStateToOthers()
    {
        foreach (NPCController npc in allNPCs)
        {
            // Skip the NPC that reached the sound position
            if (npc == this)
                continue;

            // Assign patrol state to other NPCs
            npc.ReturnToPatrol();
        }
    }


    private void MoveToSound(Vector3 _pos)
    {
        if (isDead == true)
        {
            minimapIcon.color = deathColor;
            minimapCurrentColor = minimapIcon.color;

            navMeshAgent.isStopped = false;
            alertIcon.SetActive(false);
            chaseTargetIcon.SetActive(false);
            navMeshAgent.SetDestination(_pos); // Set the destination to the position of the sound
            navMeshAgent.speed = chaseSpeed;
            animator.SetFloat(BLENDSTATE, 0.67f);

            // Start a coroutine to monitor if the NPC has reached the sound position
            //moveToSoundCoroutine = StartCoroutine(MonitorSoundPosition(_pos));
        }
        else
        {
            minimapIcon.color = alertColor;
            minimapCurrentColor = minimapIcon.color;

            navMeshAgent.isStopped = false;
            alertIcon.SetActive(true);
            chaseTargetIcon.SetActive(false);
            navMeshAgent.SetDestination(_pos); // Set the destination to the position of the sound
            navMeshAgent.speed = chaseSpeed;
            animator.SetFloat(BLENDSTATE, 0.67f);

            // Start a coroutine to monitor if the NPC has reached the sound position
            moveToSoundCoroutine = StartCoroutine(MonitorSoundPosition(_pos));
            // When reaching the sound position, assign patrol state to other NPCs
            AssignPatrolStateToOthers();

        }

    }

    private IEnumerator MonitorSoundPosition(Vector3 soundPosition)
    {
        while (Vector3.Distance(transform.position, soundPosition) > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        // NPC has reached the sound position

        animator.SetFloat(BLENDSTATE, 0f); // Switch the animator state to idle
        moveToSoundCoroutine = null;

        // Wait for a while before returning to patrol
        yield return new WaitForSeconds(10f); // Adjust this time as needed
        alertIcon.SetActive(false);
        ReturnToPatrol();
    }




}