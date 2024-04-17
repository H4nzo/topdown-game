using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System.Linq;

public enum EnemyState
{
    Idle,
    Patrol,
    Alert,
    Chase
}

[RequireComponent(typeof(NavMeshAgent))]
public class NPCcontroller : MonoBehaviour
{

    public EnemyState enemyState;

    [SerializeField] private GameObject alertIcon, chaseTargetIcon;

    private const string BLENDSTATE = "Speed";
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    [SerializeField] private float idleTimer = 0f;
    private bool isIdling = false;

    [SerializeField] private float alertDuration = 3f; // Adjust as needed
    private float alertTimer = 0f;
    [SerializeField] private bool isAlerting = false;

    public Waypoint[] waypoints;
    [SerializeField] private Waypoint currentWaypoint;
    [SerializeField] private int currentWaypointIndex = 0;

    private Transform target; 
    private bool isChasing = false;
    private bool detectPlayer = false;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed = 4.1f;

    private Vector3 lastKnownPosition;
    [SerializeField] private float chaseDistanceThreshold = 10f; 
    private float distanceCovered = 0f;

    // Start is called before the first frame update
    void Start()
    {
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

       
    }

    // Update is called once per frame
    void Update()
    {


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



        if (detectPlayer && !isChasing && enemyState != EnemyState.Alert)
        {
            StartAlerting();
        }

        if (isAlerting)
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertDuration)
            {
                alertTimer = 0f;
                isAlerting = false;
                StartChasing();
            }
        }

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

    }

    void ReturnToPatrol()
    {
        enemyState = EnemyState.Patrol;
        isChasing = false;

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
        PatrolToDestination();
    }


    void StartAlerting()
    {
        enemyState = EnemyState.Alert;
        isAlerting = true;
        animator.SetFloat(BLENDSTATE, 0f);
        navMeshAgent.isStopped = true;
        alertIcon.SetActive(true);
        Debug.Log("Alerting");
    }

    void StartChasing()
    {
        enemyState = EnemyState.Chase;
        isChasing = true;
      
        Debug.Log("Chasing");
    }

    void ChaseTarget()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
            navMeshAgent.isStopped = false;

            alertIcon.SetActive(false);
            chaseTargetIcon.SetActive(true);
            navMeshAgent.speed = chaseSpeed;
            animator.SetFloat(BLENDSTATE, 1f);
        }
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
        animator.SetFloat(BLENDSTATE, .5f);
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
            Debug.Log("Detected Player");
        }
        else
        {
            Debug.Log(gameObject.name + " did not Detect Player");
        }
    }

}
