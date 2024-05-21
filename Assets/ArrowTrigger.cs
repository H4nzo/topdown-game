using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class ArrowTrigger : MonoBehaviour
{
    public Transform player; // Reference to the player transform
    public LineRenderer lineRenderer; // Reference to the LineRenderer component

    private NavMeshPath path;
    private Vector3 lastPlayerPosition;
    private NavMeshAgent playerNavMeshAgent;

    public UnityEvent onTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onTrigger?.Invoke();
        }
    }

    void Start()
    {
        path = new NavMeshPath();
        playerNavMeshAgent = player.GetComponent<NavMeshAgent>();
        lastPlayerPosition = GetPlayerPosition();
        CalculateAndDrawPath();
    }

    void Update()
    {
        Vector3 currentPlayerPosition = GetPlayerPosition();

        // Check if the player has moved significantly
        if (Vector3.Distance(currentPlayerPosition, lastPlayerPosition) > 0.1f)
        {
            Debug.Log("Player moved, recalculating path.");
            lastPlayerPosition = currentPlayerPosition;
            CalculateAndDrawPath();
        }

        // Manual trigger for testing
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Manual path recalculation triggered.");
            CalculateAndDrawPath();
        }
    }

    void CalculateAndDrawPath()
    {
        Debug.Log("Calculating path...");
        // Calculate the path to the player
        if (NavMesh.CalculatePath(transform.position, GetPlayerPosition(), NavMesh.AllAreas, path))
        {
            Debug.Log("Path calculated successfully.");
            DrawPath();
        }
        else
        {
            Debug.LogWarning("Failed to calculate path.");
        }
    }

    void DrawPath()
    {
        Debug.Log("Drawing path...");
        if (path.corners.Length < 2)
        {
            Debug.LogWarning("Path has less than 2 corners.");
            return;
        }

        for (int i = 0; i < path.corners.Length; i++)
        {
            Debug.Log($"Corner {i}: {path.corners[i]}");
        }

        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
        Debug.Log("Path drawn with " + path.corners.Length + " corners.");
    }

    Vector3 GetPlayerPosition()
    {
        // Use the NavMeshAgent's position if available
        if (playerNavMeshAgent != null)
        {
            return playerNavMeshAgent.transform.position;
        }
        else
        {
            return player.position;
        }
    }
}
