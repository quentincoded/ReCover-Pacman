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
using System.Collections; // Required for Coroutines

public class PacmanScript : MonoBehaviour
{
    public Rigidbody2D Pacmanbody; // Reference to the Rigidbody2D component
    public float moveSpeed = 5f;   // Speed multiplier for movement
    private float moveInput;
    public LogicScript logic; // Assign this in the aInspector
    // --- Added for Feedback ---
    public SpriteRenderer pacmanSprite; // Assign Pacman's SpriteRenderer here
    public Color flashColor = Color.red; // Color to flash to
    public float flashDuration = 0.5f; // Duration of the flash effect
    public float flashInterval = 0.1f; // Time between flashes

    public AudioClip hurtSound; // Assign your hurt sound clip in the Inspector
    private AudioSource audioSource; // Reference to the AudioSource component

    private bool isInvincible = false; // To prevent losing multiple lives at once
    public float invincibilityDuration = 1.0f; // Duration of invincibility after being hit

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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
        // Check if the collided object has the tag "Ghost" and Pacman is not invincible
        if (collidedObject.CompareTag("Ghost") && !isInvincible)
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

            // --- Trigger Feedback Effects ---
            StartCoroutine(FlashEffect());
            PlayHurtSound();
            StartCoroutine(GainInvincibility());
            // ------------------------------
        }
    }

    // --- Coroutine for Flashing Effect ---
    IEnumerator FlashEffect()
    {
        Color originalColor = pacmanSprite.color;
        float timer = 0f;

        while (timer < flashDuration)
        {
            pacmanSprite.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            pacmanSprite.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2; // Add time for both flash on and off
        }

        pacmanSprite.color = originalColor; // Ensure color is reset
    }
    // -------------------------------------

    // --- Function to Play Sound ---
    void PlayHurtSound()
    {
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound); // Play the sound effect once
        }
    }
    // ------------------------------

    // --- Coroutine for Invincibility ---
    IEnumerator GainInvincibility()
    {
        isInvincible = true;
        // Optional: Add visual indication for invincibility (e.g., semi-transparent sprite)
        // pacmanSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Example semi-transparency

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        // Optional: Reset visual indication
        // pacmanSprite.color = originalColor; // Example reset transparency
    }
}
