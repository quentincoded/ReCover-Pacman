using UnityEngine;

public class Ghostmove_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5f; // Speed of the ghost
    public float deadzone_y = -10f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime; // Move the ghost to the left

        if (transform.position.y < deadzone_y) // Check if the ghost is in the dead zone
        {
            Destroy(gameObject); // Destroy the ghost if it is in the dead zone
            Debug.Log("Ghost destroyed"); // Log a message to the console
        }
    }
}
