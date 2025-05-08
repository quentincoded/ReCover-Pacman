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

    void Update()
    {
        // Read horizontal movement input
        moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
                    Keyboard.current.rightArrowKey.isPressed ? 1 : 0;

        // Apply velocity to move Pacman left or right
        Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);
    }
}
