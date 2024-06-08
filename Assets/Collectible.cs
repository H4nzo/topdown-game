using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed of rotation
    public float moveDistance = 2f;    // Distance to move up and down
    public float moveSpeed = 2f;       // Speed of the up and down movement

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate the object around the Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Move the object up and down along the Y-axis
        float move = Mathf.PingPong(Time.time * moveSpeed, moveDistance) - (moveDistance / 2f);
        transform.position = startPosition + new Vector3(0, move, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
        else if (other.CompareTag("MedKit"))
        {
            this.gameObject.SetActive(false);
        }
    }
}

