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
using System.Collections.Generic; // Required for List

public class PacmanScript : MonoBehaviour
{
    public Rigidbody2D Pacmanbody; // Reference to the Rigidbody2D component
    public float moveSpeed = 5f;   // Speed multiplier for movement
    private float moveInput;
    public LogicScript logic; // Assign this in the aInspector
    // --- Control Mode ---
    public bool useBLEControl = true; // Set this in the Inspector to switch between BLE and Keyboard

    // --- References to Managers ---
    private BLEManager bleManager;
    private UIManager uiManager;
    // ------------------------------
    
    // --- Added for Feedback ---
    // public SpriteRenderer pacmanSprite; // Assign Pacman's SpriteRenderer here
    public Color flashColor = Color.red; // Color to flash to
    public float flashDuration = 0.5f; // Duration of the flash effect
    public float flashInterval = 0.1f; // Time between flashes

    public AudioClip hurtSound; // Assign your hurt sound clip in the Inspector
    private AudioSource audioSource; // Reference to the AudioSource component

    private bool isInvincible = false; // To prevent losing multiple lives at once
    public float invincibilityDuration = 1.0f; // Duration of invincibility after being hit

    public GameObject mouthClosedSprite; // closed sprite GameObject
    public GameObject mouthQuarterClosedSprite; // nearly closed sprite GameObject
    public GameObject mouthHalfOpenSprite; // half open sprite GameObject
    public GameObject mouthQuarterOpenSprite; // mouth nearly open sprite GameObject
    public GameObject mouthOpenSprite; // mouth fully open sprite GameObject
    private List<GameObject> mouthSprites = new List<GameObject>();

    public enum MouthState { Closed,QuarterClosed, HalfOpen, QuarterOpen, Open }; //0=closed, 1=quarter closed, 2=half open, 3=quarter open, 4=open
    private MouthState currentMouthState = MouthState.Closed;

    public bool IsMouthOpen
    {
        get { return (int)currentMouthState >= (int)MouthState.HalfOpen; } // Cast to int for comparison
    }
    // --- Calibrated Sensor Ranges (Loaded from PlayerPrefs) ---
    private float minPotValue;
    private float maxPotValue;
    private float minFsrValue; 
    private float maxFsrValue; 
    private float minTofValue; 
    private float maxTofValue;
    // ----------------------------------------------------------


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
        // // Add all mouth sprite GameObjects to the list
        // mouthSprites.Add(mouthClosedSprite);
        // mouthSprites.Add(mouthQuarterClosedSprite);
        // mouthSprites.Add(mouthHalfOpenSprite);
        // mouthSprites.Add(mouthQuarterOpenSprite);
        // mouthSprites.Add(mouthOpenSprite);
        // UpdateMouthVisual();
        // --- Get Manager Instances ---
        bleManager = BLEManager.Instance;
        uiManager = UIManager.Instance; // Get UIManager instance (should be found by BLEManager on scene load)

        if (bleManager == null)
        {
            Debug.LogWarning("BLEManager instance not found. BLE control will not be available.");
            useBLEControl = false; // Fallback to keyboard if BLEManager is missing
        }

        if (uiManager == null)
        {
             Debug.LogWarning("UIManager instance not found. Debug UI will not be available.");
        }
        // -----------------------------

        // --- Load Calibrated Values ---
        LoadCalibrationValues();
        // ------------------------------

        // --- Added: Set initial mouth visual state ---
        UpdateMouthVisual();
        // ---------------------------------------------

        // --- IMPORTANT PHYSICS NOTE ---
        // To prevent Pacman from spinning on collision,
        // select the Player GameObject in the Inspector,
        // find the Rigidbody 2D component, expand "Constraints",
        // and check the "Freeze Rotation" checkbox for the Z-axis.
        // ------------------------------
        
    }


    void Update()
    {
        float currentPotValue = BLEManager.Instance.latestPotValue;
        float currentTofValue = BLEManager.Instance.latestTofValue;
        float currentFsrValue = BLEManager.Instance.latestFsrValue;
        
        int superSize = 1; 
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
            string filename = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(filename, superSize);
            Debug.Log($"Screenshot saved: {filename}");
            }

        
        // Only allow movement if the game is not paused (Time.timeScale > 0)
        if (Time.timeScale > 0f)
        {
            // --- Horizontal Movement Input ---
            moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
                        Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
            Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);
            // ---------------------------------

            // --- Mouth Opening Input (Keyboard Simulation) ---
            // Use isPressed to keep mouth open while key is held
            if (Keyboard.current.upArrowKey.isPressed)
            {
                // For simple keyboard control, set to fully open when pressed
                currentMouthState = MouthState.Open; // Or cycle through states here if desired
            }
            else
            {
                // Set to closed when key is not pressed
                currentMouthState = MouthState.Closed;
            }
            // -------------------------------------------------

            // --- Update mouth visual based on the state ---
            UpdateMouthVisual();
            // ----------------------------------------------
        }
        else
        {
             // Optionally stop Pacman completely when paused
             Pacmanbody.linearVelocity = Vector2.zero;
             // Ensure mouth is closed when game is paused
             currentMouthState = MouthState.Closed;
             UpdateMouthVisual();
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
            // Find the currently active mouth sprite's SpriteRenderer
            SpriteRenderer activeSpriteRenderer = GetActiveMouthSpriteRenderer();
            if (activeSpriteRenderer != null)
            {
                StartCoroutine(FlashEffect(activeSpriteRenderer)); // Start flashing the active sprite
            }
            PlayHurtSound();
            StartCoroutine(GainInvincibility());
            // ------------------------------
        }
    }
    // Helper method to get the SpriteRenderer of the currently active mouth sprite
    private SpriteRenderer GetActiveMouthSpriteRenderer()
    {
        foreach (GameObject mouthSprite in mouthSprites)
        {
            // Check if the GameObject is assigned and is currently active in the hierarchy
            if (mouthSprite != null && mouthSprite.activeSelf)
            {
                // Return the SpriteRenderer component from the active GameObject
                return mouthSprite.GetComponent<SpriteRenderer>();
            }
        }
        // Return null if no active mouth sprite is found
        return null;
    }

    // --- Coroutine for Flashing Effect ---
    IEnumerator FlashEffect(SpriteRenderer spriteToFlash)
    {
        if (spriteToFlash == null) yield break; // Exit if no sprite renderer is provided

        Color originalColor = spriteToFlash.color;
        float timer = 0f;

        while (timer < flashDuration)
        {
            spriteToFlash.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            spriteToFlash.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2; // Add time for both flash on and off
        }

        spriteToFlash.color = originalColor; // Ensure color is reset
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
    
    
    // --- Method to update mouth visual based on currentMouthState ---
    void UpdateMouthVisual()
    {
        // Disable all mouth sprites first
        if (mouthClosedSprite != null) mouthClosedSprite.SetActive(false);
        if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(false); // Corrected Casing
        if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(false);
        if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(false);
        if (mouthOpenSprite != null) mouthOpenSprite.SetActive(false);


        // Enable the current mouth sprite based on the state
        switch (currentMouthState)
        {
            case MouthState.Closed:
                if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true);
                break;
            case MouthState.QuarterClosed:
                if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Corrected Casing
                else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
                break;
            case MouthState.HalfOpen:
                if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true);
                 else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback (Corrected Casing)
                 else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
                break;
            case MouthState.QuarterOpen:
                 if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true);
                 else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
                 else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback (Corrected Casing)
                 else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
                break;
            case MouthState.Open:
                if (mouthOpenSprite != null) mouthOpenSprite.SetActive(true);
                else if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true); // Fallback
                else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
                else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback (Corrected Casing)
                else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
                break;
        }
        // Debug.Log("Mouth state updated to: " + currentMouthState); // Keep this for debugging if needed
    }
    
}
