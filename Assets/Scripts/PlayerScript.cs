using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerScript : MonoBehaviour
{
   
    const string BLEND = "Action";

    const string STAB = "Stab";
    const string RUN = "Run";

    NavMeshAgent agent;
    [SerializeField] float stoppingDistance = 1.2f;
    private float defaultSpeed;
    Animator animator;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] GameObject linePrefab; // Reference to the sprite prefab
    [SerializeField] Material deathTrailMaterial;
    private Vector3 lastPosition;
    private GameObject lineRenderer; // Reference to the instantiated line sprite

    [SerializeField]float lookRotationSpeed = 8f;

    public float killDistance = 10f;
    public float distanceToKill;

    bool isFollowingEnemy = false;
    public float stabAnimationDuration = 2f;

    private bool isStabbed = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        defaultSpeed = agent.speed;
    }

    void Update()
    {
        // Check for left mouse button or touch input
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector2 inputPosition = Input.GetMouseButtonDown(0) ? Input.mousePosition : Input.GetTouch(0).position;
            ClickToMove(inputPosition);

        }

        FaceTarget();
        SetAnimation();

        // Update line renderer while player is moving
        if (agent.velocity != Vector3.zero)
            UpdateLineRenderer();

        //if (isStabbed == false)
        //    agent.speed = defaultSpeed;

      
    }


    void ClickToMove(Vector2 inputPosition)
    {

        if (agent.speed == defaultSpeed)
        {
            agent.speed = defaultSpeed;
        }

       
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out hit, 100, clickableLayers))
        {
            if (hit.collider.CompareTag("Clickable"))
            {
                if (isFollowingEnemy)
                {
                    //StopCoroutine("FollowEnemy");
                    StopAllCoroutines();
                    isFollowingEnemy = false;
                    animator.SetFloat(BLEND, 0f);
                }
                
                agent.destination = hit.point;

                if (clickEffect != null)
                {
                    Instantiate(clickEffect, hit.point + new Vector3(0, .1f, 0), clickEffect.transform.rotation);
                    lastPosition = transform.position;


                }

                if (lineRenderer != null)
                    Destroy(lineRenderer);

                // Create a new line sprite and set its position to the player
                lineRenderer = Instantiate(linePrefab, transform.position, Quaternion.identity);

                NavMeshPath path = agent.path;
                lineRenderer.GetComponent<LineRenderer>().positionCount = path.corners.Length;
                for (int i = 0; i < path.corners.Length; i++)
                {
                    lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.corners[i]);
                }
            }
            else if (hit.collider.CompareTag("Enemy"))
            {

                    StartCoroutine(FollowEnemy(hit));
                      
                    if (clickEffect != null)
                    {
                        Instantiate(clickEffect, hit.point + new Vector3(0, .1f, 0), clickEffect.transform.rotation);
                        lastPosition = transform.position;
                    }

                    // Destroy the previous line sprite if it exists
                    if (lineRenderer != null)
                        Destroy(lineRenderer);

                
                lineRenderer = Instantiate(linePrefab, transform.position, Quaternion.identity);
                lineRenderer.GetComponent<LineRenderer>().material = deathTrailMaterial;

                   
                    NavMeshPath path = agent.path;
                    lineRenderer.GetComponent<LineRenderer>().positionCount = path.corners.Length;
                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.corners[i]);
                    }
            }
           
        }
      

    }


    IEnumerator FollowEnemy(RaycastHit enemy)
    {
        isFollowingEnemy = true;
        // Adjust this value based on your enemy size

        while (true)
        {
            // Set target slightly before enemy position to avoid overshoot
            Vector3 targetPosition = enemy.transform.position - enemy.transform.forward * stoppingDistance;
            agent.SetDestination(targetPosition);

            // Attack enemy when within stopping distance
            float distanceToEnemy = Vector3.Distance(transform.position, targetPosition);
            if (distanceToEnemy <= stoppingDistance && enemy.collider != null)
            {
                AttackEnemy(distanceToEnemy, killDistance, enemy);
            }

            yield return null;
        }
    }

    private void AttackEnemy(float _distanceToEnemy, float _killDistance, RaycastHit target)
    {
        distanceToKill = _distanceToEnemy;

        if (_distanceToEnemy <= _killDistance)
        {
            Debug.Log("Player in enemy distance");

            target.collider.GetComponent<NavMeshAgent>().isStopped = false;
            target.collider.GetComponent<NavMeshAgent>().speed = 0;
            animator.SetBool(STAB, true);
            agent.speed = 0;
            isStabbed = true;

            //agent.velocity = Vector3.zero;
           



            StartCoroutine(StopStabAnimation(target));
        }
    }

    IEnumerator StopStabAnimation(RaycastHit _target)
    {
        
        
        yield return new WaitForSeconds(stabAnimationDuration);
        //animator.SetFloat(BLEND, 0f);

        NPCController npc = _target.collider.GetComponent<NPCController>();
        npc.isDead = true;
        npc.minimapIcon.color = npc.deathColor;
        agent.speed = defaultSpeed;
        animator.SetBool(STAB, false);
        
        isFollowingEnemy = false;

        //agent.ResetPath();
       
        
        npc.GetComponent<TriggerSound>().PlaySound();
        
        _target.collider.GetComponent<Animator>().Play("Death");
        _target.collider.GetComponent<NavMeshAgent>().enabled = false;
        npc.fovMesh.SetActive(false);
        npc._DisableScript();
        _target.collider.GetComponent<SphereCollider>().enabled = false;

        ScoreSystem scoreSystem = GameObject.FindObjectOfType<ScoreSystem>();
        scoreSystem.AddScore(1);

        if (lineRenderer != null)
            Destroy(lineRenderer);
        StopAllCoroutines();
        

    }

    void UpdateLineRenderer()
    {
        // Update the position of the line renderer to follow the player
        if (lineRenderer !=null) {
            NavMeshPath path = agent.path;
            lineRenderer.GetComponent<LineRenderer>().positionCount = path.corners.Length;
            for (int i = 0; i < path.corners.Length; i++)
            {
                lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.corners[i]);
            }
        }
        else
        {
            return;
        }
    }

    private void FaceTarget()
    {
        if (agent.velocity != Vector3.zero)
        {
            Vector3 dir = (agent.steeringTarget - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    private void SetAnimation()
    {
        // Play RUN animation
        if (agent.velocity == Vector3.zero)
        {
            animator.SetBool(RUN, false);
            //animator.SetFloat(BLEND, 0);
            
        }
        else
        {
            //animator.SetFloat(BLEND, 0.5f);
            animator.SetBool(RUN, true);
        }
    }

}
