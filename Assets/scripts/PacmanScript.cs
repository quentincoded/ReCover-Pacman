// using UnityEngine;
// using UnityEngine.InputSystem; // Required for the new Input System
// using System.Collections; // Required for Coroutines
// using System.Collections.Generic; // Required for List

// public class PacmanScript : MonoBehaviour
// {
//     public Rigidbody2D Pacmanbody; // Reference to the Rigidbody2D component
//     public float moveSpeed = 5f;   // Speed multiplier for movement
//     private float moveInput;
//     public LogicScript logic; // Assign this in the aInspector
//     // --- Control Mode ---
//     public bool useBLEControl = true; // Set this in the Inspector to switch between BLE and Keyboard

//     // --- References to Managers ---
//     private BLEManager bleManager;
//     // REMOVED: private UIManager uiManager; // UIManager is now a persistent singleton accessed via Instance
//     // ------------------------------

//     // --- Added for Feedback ---
//     // Assign Pacman's main BODY SpriteRenderer here in the Inspector
//     // This is now primarily a fallback or for visual effects not tied to mouth state
//     public SpriteRenderer pacmanSprite;
//     public Color flashColor = Color.red; // Color to flash to
//     public float flashDuration = 0.5f; // Duration of the flash effect
//     public float flashInterval = 0.1f; // Time between flashes

//     public AudioClip hurtSound; // Assign your hurt sound clip in the Inspector
//     private AudioSource audioSource; // Reference to the AudioSource component

//     private bool isInvincible = false; // To prevent losing multiple lives at once
//     public float invincibilityDuration = 1.0f; // Duration of invincibility after being hit

//     public GameObject mouthClosedSprite; // closed sprite GameObject
//     public GameObject mouthQuarterClosedSprite; // nearly closed sprite GameObject
//     public GameObject mouthHalfOpenSprite; // half open sprite GameObject
//     public GameObject mouthQuarterOpenSprite; // mouth nearly open sprite GameObject
//     public GameObject mouthOpenSprite; // mouth fully open sprite GameObject
//     // Removed: private List<GameObject> mouthSprites = new List<GameObject>(); // This list isn't used in the current UpdateMouthVisual logic, removed

//     public enum MouthState { Closed,QuarterClosed, HalfOpen, QuarterOpen, Open }; //0=closed, 1=quarter closed, 2=half open, 3=quarter open, 4=open
//     private MouthState currentMouthState = MouthState.Closed;

//     public bool IsMouthOpen
//     {
//         get { return (int)currentMouthState >= (int)MouthState.HalfOpen; } // Cast to int for comparison
//     }
//     // --- Calibrated Sensor Ranges (Loaded from PlayerPrefs) ---
//     private float minPotValue;
//     private float maxPotValue;
//     private float minFsrValue;
//     private float maxFsrValue;
//     private float minTofValue;
//     private float maxTofValue;
//     // ----------------------------------------------------------


//     void Start()
//     {
//         // Attempt to find LogicScript if not assigned in Inspector
//         if (logic == null)
//         {
//             // logic = FindObjectOfType<LogicScript>();
//             logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
//         }

//         if (logic == null)
//         {
//             Debug.LogError("PacmanScript could not find LogicScript!");
//         }

//         // Ensure Rigidbody is assigned
//         if (Pacmanbody == null)
//         {
//             Pacmanbody = GetComponent<Rigidbody2D>();
//         }

//         // --- Removed: Ensure SpriteRenderer is assigned via GetComponent ---
//         // Rely on assigning pacmanSprite in the Inspector
//         if (pacmanSprite == null)
//         {
//              Debug.LogWarning("Pacman SpriteRenderer is not assigned in the Inspector! Flashing effect might not work.");
//         }
//         // -----------------------------------------------------------------

//         // Get or add AudioSource component
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null)
//         {
//             audioSource = gameObject.AddComponent<AudioSource>();
//         }

//         // --- Get Manager Instances ---
//         bleManager = BLEManager.Instance;
//         // REMOVED: uiManager = UIManager.Instance; // UIManager is now accessed directly via UIManager.Instance

//         if (bleManager == null)
//         {
//             Debug.LogWarning("BLEManager instance not found. BLE control and debug UI will not be available.");
//             useBLEControl = false; // Fallback to keyboard if BLEManager is missing
//         }

//         // UIManager reference is now obtained directly via UIManager.Instance
//         // if (uiManager == null)
//         // {
//         //      Debug.LogWarning("UIManager instance not found. Debug UI will not be available.");
//         // }
//         // -----------------------------

//         // --- Load Calibrated Values ---
//         LoadCalibrationValues();
//         // ------------------------------

//         // --- Added: Set initial mouth visual state ---
//         UpdateMouthVisual();
//         // ---------------------------------------------

//         // --- IMPORTANT PHYSICS NOTE ---
//         // To prevent Pacman from spinning on collision,
//         // select the Player GameObject in the Inspector,
//         // find the Rigidbody 2D component, expand "Constraints",
//         // and check the "Freeze Rotation" checkbox for the Z-axis.
//         // ------------------------------

//     }


//     void Update()
//     {

//         int superSize = 1;
//             if (Keyboard.current.pKey.wasPressedThisFrame)
//             {
//             string filename = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
//             ScreenCapture.CaptureScreenshot(filename, superSize);
//             Debug.Log($"Screenshot saved: {filename}");
//             }


//         // Only allow movement if the game is not paused (Time.timeScale > 0)
//         if (Time.timeScale > 0f)
//         {
//             if (useBLEControl && bleManager != null && bleManager.isConnected)
//             {
//                 // --- BLE Control ---
//                 ControlWithBLE();
//                 // -------------------
//             }
//             else
//             {
//                 // --- Keyboard Control (Fallback or Primary) ---
//                 ControlWithKeyboard();
//                 // ----------------------------------------------
//             }

//             // --- Update mouth visual based on the state (called by both control methods) ---
//             UpdateMouthVisual();
//             // -----------------------------------------------------------------------------

//             // --- Update Debug UI ---
//             UpdateDebugUI();
//             // -----------------------
//         }
//         else
//         {
//             // Optionally stop Pacman completely when paused
//             Pacmanbody.linearVelocity = Vector2.zero;
//             // Ensure mouth is closed when game is paused
//             currentMouthState = MouthState.Closed;
//             UpdateMouthVisual();
//             // Clear debug UI when paused - now checks UIManager.Instance
//             // UPDATED: Access UIManager via Instance
//             if (UIManager.Instance != null)
//             {
//                 UIManager.Instance.UpdateRawValues(0, 0, 0);
//                 UIManager.Instance.UpdateCalibratedRanges(0, 0, 0, 0, 0, 0); // Updated call
//                 UIManager.Instance.UpdateMappedValues(0, 0); // Updated call
//                 UIManager.Instance.UpdateMouthState(currentMouthState.ToString());
//             }
//         }
//     }
//     // -----------------------------------
//     // --- Method for Keyboard Control ---
//     void ControlWithKeyboard()
//     {
//         // Horizontal Movement
//         float moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
//                           Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
//         // Fixed: Removed .z as Vector2 does not have a z component
//         Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);

//         // Mouth Opening (Simple Open/Closed Toggle for Keyboard)
//         if (Keyboard.current.upArrowKey.isPressed)
//         {
//             currentMouthState = MouthState.Open;
//         }
//         else
//         {
//             currentMouthState = MouthState.Closed;
//         }
//     }
//     // -----------------------------------
//     // --- Method for BLE Control ---
//     void ControlWithBLE()
//     {
//         // Get latest sensor values from BLEManager
//         float rawPot = bleManager.latestPotValue;
//         float rawFsr = bleManager.latestFsrValue;
//         float rawTof = bleManager.latestTofValue;

//         // --- Horizontal Movement (Potentiometer Mapping) ---
//         // Map raw potentiometer value (minPot to maxPot) to horizontal screen position
//         // Without BorderPositioner, we use camera viewport edges as boundaries.
//         Camera mainCamera = Camera.main;
//         if (mainCamera != null)
//         {
//             // Get world coordinates of screen edges (at Pacman's Z position)
//             float screenLeftX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, transform.position.z - mainCamera.transform.position.z)).x;
//             float screenRightX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, transform.position.z - mainCamera.transform.position.z)).x;

//             // Normalize potentiometer value (0 to 1) within the calibrated range
//             // Clamp rawPot to the calibrated range before normalizing to avoid issues with values outside the range
//             float clampedPot = Mathf.Clamp(rawPot, minPotValue, maxPotValue);
//             float normalizedPot = Mathf.InverseLerp(minPotValue, maxPotValue, clampedPot);

//             // Map normalized value to the horizontal screen range
//             float targetXPosition = Mathf.Lerp(screenLeftX, screenRightX, normalizedPot);

//             // Smoothly move Pacman towards the target X position
//             // Using MoveTowards for simple smooth movement
//             // Fixed: Use transform.position.z for the z-component
//             Vector3 targetPosition = new Vector3(targetXPosition, Pacmanbody.position.y, transform.position.z);
//             // You might need to adjust the speed multiplier here based on how quickly you want Pacman to track the potentiometer
//             Pacmanbody.position = Vector3.MoveTowards(Pacmanbody.position, targetPosition, moveSpeed * Time.deltaTime);

//             // Note: We are setting position directly, not using linearVelocity for this type of control.
//             Pacmanbody.linearVelocity = Vector2.zero; // Stop any residual velocity

//             // --- IMPORTANT BORDER NOTE ---
//             // When setting transform.position directly, Unity places the object exactly there.
//             // If the targetXPosition is outside a collider (like a border), Pacman will be placed outside.
//             // The collider will prevent him from being pushed *further* out by physics forces,
//             // but won't push him back in. To ensure he stays within bounds, you should either:
//             // 1. Use Mathf.Clamp on the targetXPosition based on your border positions.
//             // 2. Implement a BorderPositioner script that moves the borders and use those calculated
//             //    positions to clamp Pacman's X position.
//             // For now, since you don't have BorderPositioner, we've mapped to screen edges.
//             // If you add fixed borders, you'll need to clamp based on their positions + half Pacman's width.
//             // Example Clamp: Pacmanbody.position = new Vector3(Mathf.Clamp(targetXPosition, minBorderX + halfPacmanWidth, maxBorderX - halfPacmanWidth), ...);
//             // -----------------------------
//         }
//         else
//         {
//              Debug.LogWarning("Main Camera not found! Cannot perform BLE horizontal mapping.");
//         }
//         // -------------------------------------------------

//         // --- Mouth Opening (Combined FSR and ToF Mapping) ---
//         // Normalize FSR value (minFsr to maxFsr)
//         // Clamp rawFsr to the calibrated range before normalizing
//         float clampedFsr = Mathf.Clamp(rawFsr, minFsrValue, maxFsrValue);
//         float normalizedFsr = Mathf.InverseLerp(minFsrValue, maxFsrValue, clampedFsr);

//         // Normalize ToF value (minTof to maxTof)
//         // Clamp rawTof to the calibrated range before normalizing
//         float clampedTof = Mathf.Clamp(rawTof, minTofValue, maxTofValue);
//         float normalizedTof = Mathf.InverseLerp(minTofValue, maxTofValue, clampedTof);

//         // Combine normalized FSR and ToF values equally (average)
//         // You can adjust the weighting (e.g., 0.7 * normalizedTof + 0.3 * normalizedFsr) if one sensor is more reliable
//         float combinedNormalizedMouth = (normalizedFsr + normalizedTof) / 2f;
//         // Clamp the combined value between 0 and 1 just in case
//         combinedNormalizedMouth = Mathf.Clamp01(combinedNormalizedMouth);


//         // Determine MouthState based on the combined normalized value (dividing 0-1 into 5 segments)
//         // Adjust these thresholds based on your calibration and desired feel
//         if (combinedNormalizedMouth < 0.2f)
//         {
//             currentMouthState = MouthState.Closed;
//         }
//         else if (combinedNormalizedMouth < 0.4f)
//         {
//             currentMouthState = MouthState.QuarterClosed;
//         }
//         else if (combinedNormalizedMouth < 0.6f)
//         {
//             currentMouthState = MouthState.HalfOpen;
//         }
//         else if (combinedNormalizedMouth < 0.8f)
//         {
//             currentMouthState = MouthState.QuarterOpen;
//         }
//         else // combinedNormalizedMouth >= 0.8f
//         {
//             currentMouthState = MouthState.Open;
//         }
//         // ------------------------------------------
//     }
//     // --------------------------------

//     // --- Load Calibration Values from PlayerPrefs ---
//     void LoadCalibrationValues()
//     {
//         // Use the static helper methods from CalibrationManagerScript
//         // Provide default values in case calibration hasn't been run or keys don't exist
//         minPotValue = CalibrationManagerScript.GetMinPotValue();
//         maxPotValue = CalibrationManagerScript.GetMaxPotValue();
//         minFsrValue = CalibrationManagerScript.GetMinFsrValue(); // Load Min FSR
//         maxFsrValue = CalibrationManagerScript.GetMaxFsrValue(); // Load Max FSR
//         minTofValue = CalibrationManagerScript.GetMinTofValue(); // Load Min ToF
//         maxTofValue = CalibrationManagerScript.GetMaxTofValue(); // Load Max ToF


//         Debug.Log($"Loaded Calibration - MinPot: {minPotValue}, MaxPot: {maxPotValue}, MinFsr: {minFsrValue}, MaxFsr: {maxFsrValue}, MinToF: {minTofValue}, MaxToF: {maxTofValue}");

//         // You might want to display these loaded values in the UI as well
//         // This is now handled in UpdateDebugUI to ensure UIManager is ready
//         // if (uiManager != null)
//         // {
//         //      uiManager.UpdateCalibratedRanges(minPotValue, maxPotValue, minFsrValue, maxFsrValue, minTofValue, maxTofValue);
//         // }
//     }
//     // ----------------------------------------------

//     // --- Update Debug UI with current sensor and mapping data ---
//     void UpdateDebugUI()
//     {
//         // UPDATED: Access UIManager directly via Instance
//         if (UIManager.Instance != null && bleManager != null)
//         {
//             // Update raw values
//             UIManager.Instance.UpdateRawValues(bleManager.latestPotValue, bleManager.latestFsrValue, bleManager.latestTofValue);

//             // Update calibrated ranges (can be called once in Start or here if they can change)
//             UIManager.Instance.UpdateCalibratedRanges(minPotValue, maxPotValue, minFsrValue, maxFsrValue, minTofValue, maxTofValue);

//             // Calculate and update mapped values
//             float rawPot = bleManager.latestPotValue;
//             float rawFsr = bleManager.latestFsrValue;
//             float rawTof = bleManager.latestTofValue;

//             float clampedPot = Mathf.Clamp(rawPot, minPotValue, maxPotValue);
//             float normalizedPot = Mathf.InverseLerp(minPotValue, maxPotValue, clampedPot);
//             float screenLeftX = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, transform.position.z - Camera.main.transform.position.z)).x : 0;
//             float screenRightX = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, transform.position.z - Camera.main.transform.position.z)).x : 0;
//             float targetXPosition = Mathf.Lerp(screenLeftX, screenRightX, normalizedPot);

//             float clampedFsr = Mathf.Clamp(rawFsr, minFsrValue, maxFsrValue);
//             float normalizedFsr = Mathf.InverseLerp(minFsrValue, maxFsrValue, clampedFsr);
//             float clampedTof = Mathf.Clamp(rawTof, minTofValue, maxTofValue);
//             float normalizedTof = Mathf.InverseLerp(minTofValue, maxTofValue, clampedTof);
//             float combinedNormalizedMouth = (normalizedFsr + normalizedTof) / 2f;


//             UIManager.Instance.UpdateMappedValues(targetXPosition, combinedNormalizedMouth); // Updated call

//             // Update mouth state
//             UIManager.Instance.UpdateMouthState(currentMouthState.ToString());
//         }
//         // Removed redundant else if (bleManager != null) block
//         else if (UIManager.Instance != null) // If BLEManager is null (e.g., not in the scene or failed to initialize) but UIManager is present
//         {
//              // If BLEManager is not available, indicate that debug data is not live
//              UIManager.Instance.UpdateRawValues(0, 0, 0);
//              UIManager.Instance.UpdateCalibratedRanges(0, 0, 0, 0, 0, 0); // Updated call
//              UIManager.Instance.UpdateMappedValues(0, 0); // Updated call
//              UIManager.Instance.UpdateMouthState(currentMouthState.ToString() + " (Keyboard)");
//         }
//     }
//     // ------------------------------------------------------------

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         Debug.Log("Trigger Pacman ghost, object " + other.gameObject.name);
//         HandleGhostCollision(other.gameObject);
//     }

//     private void HandleGhostCollision(GameObject collidedObject)
//     {
//         // Check if the collided object has the tag "Ghost" and Pacman is not invincible
//         if (collidedObject.CompareTag("Ghost") && !isInvincible)
//         {
//             Debug.Log("Collided with Ghost!");

//             // Destroy the specific ghost instance Pacman collided with
//             Destroy(collidedObject);

//             // Tell the LogicScript to decrease a life
//             if (logic != null)
//             {
//                 logic.LoseLife();
//             }
//             else
//             {
//                 Debug.LogError("LogicScript reference missing in PacmanScript!");
//             }

//             // --- Trigger Feedback Effects ---
//             // Find the currently active mouth sprite's SpriteRenderer
//             SpriteRenderer activeSpriteRenderer = GetActiveMouthSpriteRenderer();
//             // Start the FlashEffect coroutine, passing the active sprite renderer
//             if (activeSpriteRenderer != null)
//             {
//                  StartCoroutine(FlashEffect(activeSpriteRenderer)); // Pass the active sprite renderer
//             }
//             // Fallback: If no active mouth sprite renderer is found, try flashing the main pacmanSprite
//             else if (pacmanSprite != null)
//             {
//                  StartCoroutine(FlashEffect(pacmanSprite)); // Use the main pacmanSprite
//             }
//             else
//             {
//                  Debug.LogWarning("No active mouth sprite renderer or main pacmanSprite assigned for flashing effect!");
//             }


//             PlayHurtSound();
//             StartCoroutine(GainInvincibility());
//             // ------------------------------
//         }
//     }
//     // Helper method to get the SpriteRenderer of the currently active mouth sprite
//     private SpriteRenderer GetActiveMouthSpriteRenderer()
//     {
//         // List all mouth sprite GameObjects for easy iteration
//         List<GameObject> mouthSprites = new List<GameObject>
//         {
//             mouthClosedSprite,
//             mouthQuarterClosedSprite,
//             mouthHalfOpenSprite,
//             mouthQuarterOpenSprite,
//             mouthOpenSprite
//         };

//         foreach (GameObject mouthSprite in mouthSprites)
//         {
//             // Check if the GameObject is assigned and is currently active in the hierarchy
//             if (mouthSprite != null && mouthSprite.activeSelf)
//             {
//                 // Return the SpriteRenderer component from the active GameObject
//                 return mouthSprite.GetComponent<SpriteRenderer>();
//             }
//         }
//         // Return null if no active mouth sprite is found
//         return null;
//     }

//     // --- Coroutine for Flashing Effect ---
//     // Modified to accept a SpriteRenderer parameter
//     IEnumerator FlashEffect(SpriteRenderer spriteToFlash)
//     {
//         if (spriteToFlash == null) yield break; // Exit if no sprite renderer is provided

//         Color originalColor = spriteToFlash.color;
//         float timer = 0f;

//         while (timer < flashDuration)
//         {
//             spriteToFlash.color = flashColor;
//             yield return new WaitForSeconds(flashInterval);
//             spriteToFlash.color = originalColor;
//             yield return new WaitForSeconds(flashInterval);
//             timer += flashInterval * 2; // Add time for both flash on and off
//         }

//         spriteToFlash.color = originalColor; // Ensure color is reset
//     }
//     // -------------------------------------

//     // --- Function to Play Sound ---
//     void PlayHurtSound()
//     {
//         if (audioSource != null && hurtSound != null)
//         {
//             audioSource.PlayOneShot(hurtSound); // Play the sound effect once
//         }
//     }
//     // ------------------------------

//     // --- Coroutine for Invincibility ---
//     IEnumerator GainInvincibility()
//     {
//         isInvincible = true;
//         // Optional: Add visual indication for invincibility (e.g., semi-transparent sprite)
//         // if (pacmanSprite != null) // Check if assigned before changing color
//         // {
//         //     Color originalColor = pacmanSprite.color; // Need to get original color if changing transparency here
//         //     pacmanSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Example semi-transparency
//         // }


//         yield return new WaitForSeconds(invincibilityDuration);

//         isInvincible = false;
//         // Optional: Reset visual indication
//         // if (pacmanSprite != null) // Check if assigned before changing color
//         // {
//         //      pacmanSprite.color = originalColor; // Example reset transparency
//         // }
//     }


//     // --- Method to update mouth visual based on currentMouthState ---
//     void UpdateMouthVisual()
//     {
//         // Disable all mouth sprites first
//         if (mouthClosedSprite != null) mouthClosedSprite.SetActive(false);
//         if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(false);
//         if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(false);
//         if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(false);
//         if (mouthOpenSprite != null) mouthOpenSprite.SetActive(false);


//         // Enable the current mouth sprite based on the state
//         switch (currentMouthState)
//         {
//             case MouthState.Closed:
//                 if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true);
//                 break;
//             case MouthState.QuarterClosed:
//                 if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true);
//                 else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
//                 break;
//             case MouthState.HalfOpen:
//                 if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true);
//                  else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
//                  else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
//                 break;
//             case MouthState.QuarterOpen:
//                  if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true);
//                  else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
//                  else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
//                  else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
//                 break;
//             case MouthState.Open:
//                 if (mouthOpenSprite != null) mouthOpenSprite.SetActive(true);
//                 else if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true); // Fallback
//                 else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
//                 else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
//                 else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
//                 break;
//         }
//         // Debug.Log("Mouth state updated to: " + currentMouthState); // Keep this for debugging if needed
//     }

// }

//old versio with five sprites

//----------------

//new version with two sprites

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
    // REMOVED: private UIManager uiManager; // UIManager is now a persistent singleton accessed via Instance
    // ------------------------------

    // --- Added for Feedback ---
    // Assign Pacman's main BODY SpriteRenderer here in the Inspector
    // This is now primarily a fallback or for visual effects not tied to mouth state
    public SpriteRenderer pacmanSprite;
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
    // Removed: private List<GameObject> mouthSprites = new List<GameObject>(); // This list isn't used in the current UpdateMouthVisual logic, removed

    public enum MouthState { Closed, Open }; // 0=closed, 1=open
    //for five sprites:
    // public enum MouthState { Closed,QuarterClosed, HalfOpen, QuarterOpen, Open }; //0=closed, 1=quarter closed, 2=half open, 3=quarter open, 4=open

    private MouthState currentMouthState = MouthState.Closed;

    public bool IsMouthOpen
    {
        get { return currentMouthState == MouthState.Open; }
        // get { return (int)currentMouthState >= (int)MouthState.HalfOpen; } // Cast to int for comparison
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

        // --- Removed: Ensure SpriteRenderer is assigned via GetComponent ---
        // Rely on assigning pacmanSprite in the Inspector
        if (pacmanSprite == null)
        {
            Debug.LogWarning("Pacman SpriteRenderer is not assigned in the Inspector! Flashing effect might not work.");
        }
        // -----------------------------------------------------------------

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // --- Get Manager Instances ---
        bleManager = BLEManager.Instance;
        // REMOVED: uiManager = UIManager.Instance; // UIManager is now accessed directly via UIManager.Instance

        if (bleManager == null)
        {
            Debug.LogWarning("BLEManager instance not found. BLE control and debug UI will not be available.");
            useBLEControl = false; // Fallback to keyboard if BLEManager is missing
        }

        // UIManager reference is now obtained directly via UIManager.Instance
        // if (uiManager == null)
        // {
        //      Debug.LogWarning("UIManager instance not found. Debug UI will not be available.");
        // }
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
            if (useBLEControl && bleManager != null && bleManager.isConnected)
            {
                // --- BLE Control ---
                ControlWithBLE();
                // -------------------
            }
            else
            {
                // --- Keyboard Control (Fallback or Primary) ---
                ControlWithKeyboard();
                // ----------------------------------------------
            }

            // --- Update mouth visual based on the state (called by both control methods) ---
            UpdateMouthVisual();
            // -----------------------------------------------------------------------------

            // --- Update Debug UI ---
            UpdateDebugUI();
            // -----------------------
        }
        else
        {
            // Optionally stop Pacman completely when paused
            Pacmanbody.linearVelocity = Vector2.zero;
            // Ensure mouth is closed when game is paused
            currentMouthState = MouthState.Closed;
            UpdateMouthVisual();
            // Clear debug UI when paused - now checks UIManager.Instance
            // UPDATED: Access UIManager via Instance
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateRawValues(0, 0, 0);
                UIManager.Instance.UpdateCalibratedRanges(0, 0, 0, 0, 0, 0); // Updated call
                UIManager.Instance.UpdateMappedValues(0, 0); // Updated call
                UIManager.Instance.UpdateMouthState(currentMouthState.ToString());
            }
        }
    }
    // -----------------------------------
    // --- Method for Keyboard Control ---
    void ControlWithKeyboard()
    {
        // Horizontal Movement
        float moveInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
                          Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
        // Fixed: Removed .z as Vector2 does not have a z component
        Pacmanbody.linearVelocity = new Vector2(moveInput * moveSpeed, Pacmanbody.linearVelocity.y);

        // Mouth Opening (Simple Open/Closed Toggle for Keyboard)
        if (Keyboard.current.upArrowKey.isPressed)
        {
            currentMouthState = MouthState.Open;
        }
        else
        {
            currentMouthState = MouthState.Closed;
        }
    }
    // -----------------------------------
    // --- Method for BLE Control ---
    void ControlWithBLE()
    {
        // Get latest sensor values from BLEManager
        float rawPot = bleManager.latestPotValue;
        float rawFsr = bleManager.latestFsrValue;
        float rawTof = bleManager.latestTofValue;

        // --- Horizontal Movement (Potentiometer Mapping) ---
        // Map raw potentiometer value (minPot to maxPot) to horizontal screen position
        // Without BorderPositioner, we use camera viewport edges as boundaries.
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Get world coordinates of screen edges (at Pacman's Z position)
            float screenLeftX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, transform.position.z - mainCamera.transform.position.z)).x;
            float screenRightX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, transform.position.z - mainCamera.transform.position.z)).x;

            // Normalize potentiometer value (0 to 1) within the calibrated range
            // Clamp rawPot to the calibrated range before normalizing to avoid issues with values outside the range
            float clampedPot = Mathf.Clamp(rawPot, minPotValue, maxPotValue);
            float normalizedPot = Mathf.InverseLerp(minPotValue, maxPotValue, clampedPot);

            // Map normalized value to the horizontal screen range
            float targetXPosition = Mathf.Lerp(screenLeftX, screenRightX, normalizedPot);

            // Smoothly move Pacman towards the target X position
            // Using MoveTowards for simple smooth movement
            // Fixed: Use transform.position.z for the z-component
            Vector3 targetPosition = new Vector3(targetXPosition, Pacmanbody.position.y, transform.position.z);
            // You might need to adjust the speed multiplier here based on how quickly you want Pacman to track the potentiometer
            Pacmanbody.position = Vector3.MoveTowards(Pacmanbody.position, targetPosition, moveSpeed * Time.deltaTime);

            // Note: We are setting position directly, not using linearVelocity for this type of control.
            Pacmanbody.linearVelocity = Vector2.zero; // Stop any residual velocity

            // --- IMPORTANT BORDER NOTE ---
            // When setting transform.position directly, Unity places the object exactly there.
            // If the targetXPosition is outside a collider (like a border), Pacman will be placed outside.
            // The collider will prevent him from being pushed *further* out by physics forces,
            // but won't push him back in. To ensure he stays within bounds, you should either:
            // 1. Use Mathf.Clamp on the targetXPosition based on your border positions.
            // 2. Implement a BorderPositioner script that moves the borders and use those calculated
            //    positions to clamp Pacman's X position.
            // For now, since you don't have BorderPositioner, we've mapped to screen edges.
            // If you add fixed borders, you'll need to clamp based on their positions + half Pacman's width.
            // Example Clamp: Pacmanbody.position = new Vector3(Mathf.Clamp(targetXPosition, minBorderX + halfPacmanWidth, maxBorderX - halfPacmanWidth), ...);
            // -----------------------------
        }
        else
        {
            Debug.LogWarning("Main Camera not found! Cannot perform BLE horizontal mapping.");
        }
        // -------------------------------------------------

        // --- Mouth Opening (Combined FSR and ToF Mapping) ---
        // Normalize FSR value (minFsr to maxFsr)
        // Clamp rawFsr to the calibrated range before normalizing
        float clampedFsr = Mathf.Clamp(rawFsr, minFsrValue, maxFsrValue);
        float normalizedFsr = Mathf.InverseLerp(minFsrValue, maxFsrValue, clampedFsr);

        // Normalize ToF value (minTof to maxTof)
        // Clamp rawTof to the calibrated range before normalizing
        float clampedTof = Mathf.Clamp(rawTof, minTofValue, maxTofValue);
        float normalizedTof = Mathf.InverseLerp(minTofValue, maxTofValue, clampedTof);

        // Combine normalized FSR and ToF values equally (average)
        // You can adjust the weighting (e.g., 0.7 * normalizedTof + 0.3 * normalizedFsr) if one sensor is more reliable
        float combinedNormalizedMouth = (normalizedFsr + normalizedTof) / 2f;
        // Clamp the combined value between 0 and 1 just in case
        combinedNormalizedMouth = Mathf.Clamp01(combinedNormalizedMouth);


        // Determine MouthState based on the combined normalized value (dividing 0-1 into 5 segments)
        // Adjust these thresholds based on your calibration and desired feel
        // for five different sprites:
        // if (combinedNormalizedMouth < 0.2f)
        // {
        //     currentMouthState = MouthState.Closed;
        // }
        // else if (combinedNormalizedMouth < 0.4f)
        // {
        //     currentMouthState = MouthState.QuarterClosed;
        // }
        // else if (combinedNormalizedMouth < 0.6f)
        // {
        //     currentMouthState = MouthState.HalfOpen;
        // }
        // else if (combinedNormalizedMouth < 0.8f)
        // {
        //     currentMouthState = MouthState.QuarterOpen;
        // }
        // else // combinedNormalizedMouth >= 0.8f
        // {
        //     currentMouthState = MouthState.Open;
        // }
        //for only two sprites:
        if (combinedNormalizedMouth >= 0.5f)
        {
            currentMouthState = MouthState.Open;
        }
        else // combinedNormalizedMouth < 0.5f
        {
            currentMouthState = MouthState.Closed;
        }
        // ------------------------------------------
    }
    // --------------------------------

    // --- Load Calibration Values from PlayerPrefs ---
    void LoadCalibrationValues()
    {
        // Use the static helper methods from CalibrationManagerScript
        // Provide default values in case calibration hasn't been run or keys don't exist
        minPotValue = CalibrationManagerScript.GetMinPotValue();
        maxPotValue = CalibrationManagerScript.GetMaxPotValue();
        minFsrValue = CalibrationManagerScript.GetMinFsrValue(); // Load Min FSR
        maxFsrValue = CalibrationManagerScript.GetMaxFsrValue(); // Load Max FSR
        minTofValue = CalibrationManagerScript.GetMinTofValue(); // Load Min ToF
        maxTofValue = CalibrationManagerScript.GetMaxTofValue(); // Load Max ToF


        Debug.Log($"Loaded Calibration - MinPot: {minPotValue}, MaxPot: {maxPotValue}, MinFsr: {minFsrValue}, MaxFsr: {maxFsrValue}, MinToF: {minTofValue}, MaxToF: {maxTofValue}");

        // You might want to display these loaded values in the UI as well
        // This is now handled in UpdateDebugUI to ensure UIManager is ready
        // if (uiManager != null)
        // {
        //      uiManager.UpdateCalibratedRanges(minPotValue, maxPotValue, minFsrValue, maxFsrValue, minTofValue, maxTofValue);
        // }
    }
    // ----------------------------------------------

    // --- Update Debug UI with current sensor and mapping data ---
    void UpdateDebugUI()
    {
        // UPDATED: Access UIManager directly via Instance
        if (UIManager.Instance != null && bleManager != null)
        {
            // Update raw values
            UIManager.Instance.UpdateRawValues(bleManager.latestPotValue, bleManager.latestFsrValue, bleManager.latestTofValue);

            // Update calibrated ranges (can be called once in Start or here if they can change)
            UIManager.Instance.UpdateCalibratedRanges(minPotValue, maxPotValue, minFsrValue, maxFsrValue, minTofValue, maxTofValue);

            // Calculate and update mapped values
            float rawPot = bleManager.latestPotValue;
            float rawFsr = bleManager.latestFsrValue;
            float rawTof = bleManager.latestTofValue;

            float clampedPot = Mathf.Clamp(rawPot, minPotValue, maxPotValue);
            float normalizedPot = Mathf.InverseLerp(minPotValue, maxPotValue, clampedPot);
            float screenLeftX = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, transform.position.z - Camera.main.transform.position.z)).x : 0;
            float screenRightX = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, transform.position.z - Camera.main.transform.position.z)).x : 0;
            float targetXPosition = Mathf.Lerp(screenLeftX, screenRightX, normalizedPot);

            float clampedFsr = Mathf.Clamp(rawFsr, minFsrValue, maxFsrValue);
            float normalizedFsr = Mathf.InverseLerp(minFsrValue, maxFsrValue, clampedFsr);
            float clampedTof = Mathf.Clamp(rawTof, minTofValue, maxTofValue);
            float normalizedTof = Mathf.InverseLerp(minTofValue, maxTofValue, clampedTof);
            float combinedNormalizedMouth = (normalizedFsr + normalizedTof) / 2f;


            UIManager.Instance.UpdateMappedValues(targetXPosition, combinedNormalizedMouth); // Updated call

            // Update mouth state
            UIManager.Instance.UpdateMouthState(currentMouthState.ToString());
        }
        // Removed redundant else if (bleManager != null) block
        else if (UIManager.Instance != null) // If BLEManager is null (e.g., not in the scene or failed to initialize) but UIManager is present
        {
            // If BLEManager is not available, indicate that debug data is not live
            UIManager.Instance.UpdateRawValues(0, 0, 0);
            UIManager.Instance.UpdateCalibratedRanges(0, 0, 0, 0, 0, 0); // Updated call
            UIManager.Instance.UpdateMappedValues(0, 0); // Updated call
            UIManager.Instance.UpdateMouthState(currentMouthState.ToString() + " (Keyboard)");
        }
    }
    // ------------------------------------------------------------

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
            // Start the FlashEffect coroutine, passing the active sprite renderer
            if (activeSpriteRenderer != null)
            {
                StartCoroutine(FlashEffect(activeSpriteRenderer)); // Pass the active sprite renderer
            }
            // Fallback: If no active mouth sprite renderer is found, try flashing the main pacmanSprite
            else if (pacmanSprite != null)
            {
                StartCoroutine(FlashEffect(pacmanSprite)); // Use the main pacmanSprite
            }
            else
            {
                Debug.LogWarning("No active mouth sprite renderer or main pacmanSprite assigned for flashing effect!");
            }


            PlayHurtSound();
            StartCoroutine(GainInvincibility());
            // ------------------------------
        }
    }
    // Helper method to get the SpriteRenderer of the currently active mouth sprite
    private SpriteRenderer GetActiveMouthSpriteRenderer()
    {
        // List all mouth sprite GameObjects for easy iteration
        List<GameObject> mouthSprites = new List<GameObject>
        {
            mouthClosedSprite,
            mouthQuarterClosedSprite,
            mouthHalfOpenSprite,
            mouthQuarterOpenSprite,
            mouthOpenSprite
        };

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
    // Modified to accept a SpriteRenderer parameter
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
        // if (pacmanSprite != null) // Check if assigned before changing color
        // {
        //     Color originalColor = pacmanSprite.color; // Need to get original color if changing transparency here
        //     pacmanSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Example semi-transparency
        // }


        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        // Optional: Reset visual indication
        // if (pacmanSprite != null) // Check if assigned before changing color
        // {
        //      pacmanSprite.color = originalColor; // Example reset transparency
        // }
    }


    // --- Method to update mouth visual based on currentMouthState ---
    void UpdateMouthVisual()
    {
        // Disable all mouth sprites first
        if (mouthClosedSprite != null) mouthClosedSprite.SetActive(false);
        if (mouthOpenSprite != null) mouthOpenSprite.SetActive(false);


        // Enable the current mouth sprite based on the state
        switch (currentMouthState)
        {
            case MouthState.Closed:
                if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true);
                break;
            case MouthState.Open:
                if (mouthOpenSprite != null) mouthOpenSprite.SetActive(true);
                break;
        }
        // Debug.Log("Mouth state updated to: " + currentMouthState); // Keep this for debugging if needed
    }
    // void UpdateMouthVisual() for five different sprites
    // {
    //     // Disable all mouth sprites first
    //     if (mouthClosedSprite != null) mouthClosedSprite.SetActive(false);
    //     if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(false);
    //     if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(false);
    //     if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(false);
    //     if (mouthOpenSprite != null) mouthOpenSprite.SetActive(false);


    //     // Enable the current mouth sprite based on the state
    //     switch (currentMouthState)
    //     {
    //         case MouthState.Closed:
    //             if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true);
    //             break;
    //         case MouthState.QuarterClosed:
    //             if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true);
    //             else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
    //             break;
    //         case MouthState.HalfOpen:
    //             if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true);
    //              else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
    //              else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
    //             break;
    //         case MouthState.QuarterOpen:
    //              if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true);
    //              else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
    //              else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
    //              else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
    //             break;
    //         case MouthState.Open:
    //             if (mouthOpenSprite != null) mouthOpenSprite.SetActive(true);
    //             else if (mouthQuarterOpenSprite != null) mouthQuarterOpenSprite.SetActive(true); // Fallback
    //             else if (mouthHalfOpenSprite != null) mouthHalfOpenSprite.SetActive(true); // Fallback
    //             else if (mouthQuarterClosedSprite != null) mouthQuarterClosedSprite.SetActive(true); // Fallback
    //             else if (mouthClosedSprite != null) mouthClosedSprite.SetActive(true); // Fallback
    //             break;
    //     }
    //     // Debug.Log("Mouth state updated to: " + currentMouthState); // Keep this for debugging if needed
    // }

}
