// using UnityEngine;
// using UnityEngine.AI;

// [RequireComponent(typeof(NavMeshAgent))]
// public class PlayerScript : MonoBehaviour
// {
//     const string RUN = "Run";

//     NavMeshAgent agent;
//     Animator animator;

//     [Header("Movement")]
//     [SerializeField] ParticleSystem clickEffect;
//     [SerializeField] LayerMask clickableLayers;

//     private Vector3 lastPosition;
    
//     float lookRotationSpeed = 8f;

//     void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         animator = GetComponent<Animator>();
//     }

//     void Update()
//     {
//         // Check for left mouse button or touch input
//         if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//         {
//             Vector2 inputPosition = Input.GetMouseButtonDown(0) ? Input.mousePosition : Input.GetTouch(0).position;
//             ClickToMove(inputPosition);
//         }

//         FaceTarget();
//         SetAnimation();
//     }

//     void ClickToMove(Vector2 inputPosition)
//     {
//         RaycastHit hit;
//         Ray ray = Camera.main.ScreenPointToRay(inputPosition);

//         if (Physics.Raycast(ray, out hit, 100, clickableLayers))
//         {
//             agent.destination = hit.point;

//             if (clickEffect != null)
//             {
//                 Instantiate(clickEffect, hit.point + new Vector3(0, .1f, 0), clickEffect.transform.rotation);
//                 lastPosition = transform.position;
//             }
//         }
//     }

//     private void FaceTarget()
//     {
//         if (agent.velocity != Vector3.zero)
//         {
//             Vector3 dir = (agent.steeringTarget - transform.position).normalized;
//             Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
//             transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
//         }
//     }

//     private void SetAnimation()
//     {
//         // Play RUN animation
//         if (agent.velocity == Vector3.zero)
//         {
//             animator.SetBool(RUN, false);
//         }
//         else
//         {
//             animator.Play("Run");
//             animator.SetBool(RUN, true);
//         }
//     }

//     void OnDrawGizmos()
// {
//     if (agent != null && agent.hasPath)
//     {
//         NavMeshPath path = agent.path;
//         Vector3[] corners = path.corners;
        
//         Gizmos.color = Color.blue;
        
//         if (corners.Length < 2)
//             return;
        
//         Vector3 previousCorner = corners[0];
//         for (int i = 1; i < corners.Length; i++)
//         {
//             Vector3 currentCorner = corners[i];
//             Gizmos.DrawLine(previousCorner, currentCorner);
//             previousCorner = currentCorner;
//         }
//     }
// }

// }

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerScript : MonoBehaviour
{
    const string RUN = "Run";

    NavMeshAgent agent;
    Animator animator;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] GameObject linePrefab; // Reference to the sprite prefab

    private Vector3 lastPosition;
    private GameObject lineRenderer; // Reference to the instantiated line sprite

    float lookRotationSpeed = 8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
    }

    void ClickToMove(Vector2 inputPosition)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out hit, 100, clickableLayers))
        {
            agent.destination = hit.point;

            if (clickEffect != null)
            {
                Instantiate(clickEffect, hit.point + new Vector3(0, .1f, 0), clickEffect.transform.rotation);
                lastPosition = transform.position;
            }

            // Destroy the previous line sprite if it exists
            if (lineRenderer != null)
                Destroy(lineRenderer);

            // Create a new line sprite and set its position to the player
            lineRenderer = Instantiate(linePrefab, transform.position, Quaternion.identity);

            // Set the points of the line renderer to follow the path
            NavMeshPath path = agent.path;
            lineRenderer.GetComponent<LineRenderer>().positionCount = path.corners.Length;
            for (int i = 0; i < path.corners.Length; i++)
            {
                lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.corners[i]);
            }
        }
    }

    void UpdateLineRenderer()
    {
        // Update the position of the line renderer to follow the player
        NavMeshPath path = agent.path;
        lineRenderer.GetComponent<LineRenderer>().positionCount = path.corners.Length;
        for (int i = 0; i < path.corners.Length; i++)
        {
            lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.corners[i]);
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
        }
        else
        {
            animator.Play("Run");
            animator.SetBool(RUN, true);
        }
    }
}
