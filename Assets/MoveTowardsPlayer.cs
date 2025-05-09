using UnityEngine;

public class MoveTowardPlayer : MonoBehaviour
{
    // public float moveSpeed = 5f; // Speed of the ghost
    public float deadzone_y = -10f;
    void Update()
    {
        
        transform.Translate(Vector3.down * GameManager.Instance.gameSpeed * Time.deltaTime);

        if (transform.position.y < deadzone_y) // Check if the object is in the dead zone
        {
            Destroy(gameObject); // Destroy the ghost if it is in the dead zone
            Debug.Log("object destroyed"); // Log a message to the console
        }
    }
}
