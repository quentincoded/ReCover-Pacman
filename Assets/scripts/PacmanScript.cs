// using UnityEngine;
// using UnityEngine.InputSystem; // Required for the new Input System

// public class PacmanScript : MonoBehaviour
// {
//     public Rigidbody2D Pacmanbody; // Reference to the Rigidbody2D component
//     public float movespeed = 5f; // Speed of Pacman
//     private float moveInput;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // Read horizontal movement input
//         moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
//                     Keyboard.current.rightArrowKey.isPressed ? 1 : 0;

//         // Apply velocity to move Pacman left or right
//         Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);
//     }
// }
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class PacmanScript : MonoBehaviour
{
    public Rigidbody2D Pacmanbody; // Reference to the Rigidbody2D component
    public float moveSpeed = 5f;   // Speed multiplier for movement
    private float moveInput;
    public LogicScript logic; // Assign this in the aInspector

    void Start()
    {
        // Attempt to find LogicScript if not assigned in Inspector
        if (logic == null)
        {
            // logic = FindObjectOfType<LogicScript>();
            logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        }

        if (logic == null)
        {
            Debug.LogError("PacmanScript could not find LogicScript!");
        }

        // Ensure Rigidbody is assigned
        if (Pacmanbody == null)
        {
            Pacmanbody = GetComponent<Rigidbody2D>();
        }
    }


    void Update()
    {
        // Only allow movement if the game is not paused (Time.timeScale > 0)
        if (Time.timeScale > 0f)
        {
            moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
                        Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
            // Use velocity for potentially smoother physics interaction
            Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);
        }
        else
        {
             // Optionally stop Pacman completely when paused
             Pacmanbody.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Pacman ghost, object " + other.gameObject.name);
        HandleGhostCollision(other.gameObject);
    }

    private void HandleGhostCollision(GameObject collidedObject)
    {
        // Check if the collided object has the tag "Ghost"
        if (collidedObject.CompareTag("Ghost"))
        {
            Debug.Log("Collided with Ghost!");

            // Destroy the specific ghost instance Pacman collided with
            Destroy(collidedObject);

            // Tell the LogicScript to decrease a life
            if (logic != null)
            {
                logic.LoseLife();
            }
            else
            {
                Debug.LogError("LogicScript reference missing in PacmanScript!");
            }

            // === Optional Enhancements ===
            // - Play a 'hurt' sound effect
            // - Trigger a visual effect (e.g., flashing Pacman sprite)
            // - Add temporary invincibility (requires more state tracking)
            // - Apply knockback to Pacman (e.g., Pacmanbody.AddForce(...))
        }
    }
}
