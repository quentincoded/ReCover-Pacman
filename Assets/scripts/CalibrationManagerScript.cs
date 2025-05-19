// using UnityEngine;
// using UnityEngine.UI; // Required for Slider/Image for progress bar
// using TMPro; // Required for TextMeshProUGUI
// using System.Collections; // Required for Coroutines
// using System.Collections.Generic; // Required for List (if averaging)
// using UnityEngine.SceneManagement; // Required for SceneManager
// using System.Linq; // Required for LINQ operations

// public class CalibrationManagerScript : MonoBehaviour
// {
//     // --- UI References ---
//     public TextMeshProUGUI instructionText;
//     public TextMeshProUGUI rawValuesText; // Optional
//     public Slider progressBar; // Or public Image progressBarFill;

//     // --- Calibration Settings ---
//     public float dataCollectionDuration = 15.0f; // How long to collect data for each step
//     public string nextSceneName = "MainGameScene"; // Name of the scene to load after calibration

//     // --- Private Variables ---
//     private BLEManager bleManager; // Reference to your BLE Manager takes the instance of the BLEManager from main menu (singleton)
//     private enum CalibrationState { NotStarted, PotLeft, PotRight, FingerRest ,FingerExtend, Complete, Saving, Error }
//     private CalibrationState currentState = CalibrationState.NotStarted;

//     private List<float> potValues = new List<float>();
//     private List<float> fsrValues = new List<float>();
//     private List<float> tofValues = new List<float>();

//     // --- PlayerPrefs Keys ---
//     private const string MinPotKey = "MinPotValue";
//     private const string MaxPotKey = "MaxPotValue";
//     private const string MinFsrKey = "MinFsrValue";
//     private const string MaxFsrKey = "MaxFsrValue";
//     private const string MinTofKey = "MinTofValue";
//     private const string MaxTofKey = "MaxTofValue";

//     void Start()
//     {
//         // Get the BLEManager instance
//         bleManager = BLEManager.Instance;
//         if (bleManager == null)
//         {
//             Debug.LogError("BLEManager instance not found! Ensure it's in the scene and set as a Singleton.");
//             SetState(CalibrationState.Error, "BLE Manager not found. Cannot calibrate.");
//             return; // Stop if BLEManager is missing
//         }

//         // Initialize UI and start calibration flow
//         if (progressBar != null)
//         {
//             progressBar.minValue = 0;
//             progressBar.maxValue = 5; // 
//             progressBar.value = 0;
//         }

//         SetState(CalibrationState.NotStarted, "Please ensure your device is connected. Starting calibration shortly...");
//         StartCoroutine(StartCalibrationFlow());
//     }

//     void Update()
//     {
//         // Optional: Update raw values display
//         if (rawValuesText != null && bleManager != null && bleManager.isConnected)
//         {
//             rawValuesText.text = $"Pot: {bleManager.latestPotValue:F2}, FSR: {bleManager.latestFsrValue:F2}, ToF: {bleManager.latestTofValue:F2}";
//         }
//         else if (rawValuesText != null)
//         {
//             rawValuesText.text = "Waiting for BLE Connection...";
//         }
//     }

//     // --- Calibration Flow Coroutine ---
//     IEnumerator StartCalibrationFlow()
//     {
//         // Give a moment for connection/start
//         yield return new WaitForSeconds(6.0f); // Adjust delay as needed

//         // Wait until connected before starting steps
//         while (bleManager == null || !bleManager.isConnected)
//         {
//              SetState(CalibrationState.NotStarted, "Connecting to device...");
//              yield return null; // Wait a frame
//         }

//         SetState(CalibrationState.PotLeft, "Step 1/4: Rotate potentiometer fully LEFT and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
//         yield return CollectDataForStep(CalibrationState.PotLeft);

//         SetState(CalibrationState.PotRight, "Step 2/4: Rotate potentiometer fully RIGHT and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
//         yield return CollectDataForStep(CalibrationState.PotRight);

//         SetState(CalibrationState.FingerRest, "Step 3/4: Relax your fingers fully and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds."); // Clarified instruction
//         yield return CollectDataForStep(CalibrationState.FingerRest);
        
//         SetState(CalibrationState.FingerExtend, "Step 4/4: EXTEND your fingers fully and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
//         yield return CollectDataForStep(CalibrationState.FingerExtend);

//         SetState(CalibrationState.Complete, "Calibration Complete!");
//         SaveCalibrationData();

//         yield return new WaitForSeconds(10.0f); // Show completion message for a bit

//         // or load tutorial scene here???
//         SceneManager.LoadSceneAsync(1); //main game
//     }

//     // --- Data Collection Coroutine for a single step ---
//     IEnumerator CollectDataForStep(CalibrationState step)
//     {
//         // Clear previous data
//         potValues.Clear();
//         fsrValues.Clear();
//         tofValues.Clear();

//         float timer = 0f;
//         while (timer < dataCollectionDuration)
//         {
//             // Collect current values
//             potValues.Add(bleManager.latestPotValue);
//             fsrValues.Add(bleManager.latestFsrValue);
//             tofValues.Add(bleManager.latestTofValue);

//             timer += Time.deltaTime;
//             yield return null; // Wait for the next frame
//         }

//         // Process collected data based on the step
//         ProcessCollectedData(step);

//         // Update progress bar
//         UpdateProgressBar(step);
//     }

//     // --- Process data after collection for a step ---
    
//     void ProcessCollectedData(CalibrationState step)
//     {
//         switch (step)
//         {
//             case CalibrationState.PotLeft:
//                 // Calculate the average of collected potentiometer values
//                 if (potValues.Count > 0)
//                 {
//                     float averageMinPot = potValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MinPotKey, averageMinPot);
//                     Debug.Log($"Calibrated Min Pot (Average): {averageMinPot}");
//                 }
//                 else
//                 {
//                      Debug.LogWarning("No potentiometer data collected for PotLeft step.");
//                 }
//                 break;

//             case CalibrationState.PotRight:
//                  // Calculate the average of collected potentiometer values
//                 if (potValues.Count > 0)
//                 {
//                     float averageMaxPot = potValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MaxPotKey, averageMaxPot);
//                     Debug.Log($"Calibrated Max Pot (Average): {averageMaxPot}");
//                 }
//                  else
//                 {
//                      Debug.LogWarning("No potentiometer data collected for PotRight step.");
//                 }
//                 break;
//             case CalibrationState.FingerRest:
//                 // Calculate the average of collected FSR and TOF values
//                 if (fsrValues.Count > 0)
//                 {
//                     float averageMinFsr = fsrValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MinFsrKey, averageMinFsr);
//                     Debug.Log($"Calibrated Min FSR (Average): {averageMinFsr}");
//                 }
//                 else
//                 {
//                      Debug.LogWarning("No FSR data collected for FingerRest step.");
//                 }
//                 if (tofValues.Count > 0)
//                 {
//                     float averageMinTof = tofValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MinTofKey, averageMinTof);
//                     Debug.Log($"Calibrated Max ToF (Average): {averageMinTof}");
//                 }
//                  else
//                 {
//                      Debug.LogWarning("No TOF data collected for FingerRest step.");
//                 }
//                 break;

//             case CalibrationState.FingerExtend:
//                 // Calculate the average of collected FSR and TOF values
//                 if (fsrValues.Count > 0)
//                 {
//                     float averageMaxFsr = fsrValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MaxFsrKey, averageMaxFsr);
//                     Debug.Log($"Calibrated Max FSR (Average): {averageMaxFsr}");
//                 }
//                  else
//                 {
//                      Debug.LogWarning("No FSR data collected for FingerExtend step.");
//                 }
//                  if (tofValues.Count > 0)
//                 {
//                     float averageMaxTof = tofValues.Average(); // Use LINQ Average()
//                     PlayerPrefs.SetFloat(MaxTofKey, averageMaxTof);
//                     Debug.Log($"Calibrated Max ToF (Average): {averageMaxTof}");
//                 }
//                  else
//                 {
//                      Debug.LogWarning("No TOF data collected for FingerExtend step.");
//                 }
//                 break;
//         }
//     }
//     // --- Save all PlayerPrefs (optional, can save after each step or at the end) ---
//     void SaveCalibrationData()
//     {
//         PlayerPrefs.Save(); // Ensure data is written to disk
//         Debug.Log("Calibration data saved to PlayerPrefs.");
//     }

//     // --- Update UI State and Instruction Text ---
//     void SetState(CalibrationState newState, string instruction)
//     {
//         currentState = newState;
//         if (instructionText != null)
//         {
//             instructionText.text = instruction;
//         }
//         Debug.Log($"Calibration State: {currentState} - {instruction}");

//         // You could also enable/disable different UI elements based on the state here
//     }

//     // --- Update Progress Bar ---
//     void UpdateProgressBar(CalibrationState completedStep)
//     {
//         if (progressBar != null)
//         {
//             // Map enum value to progress bar value (assuming enum order matches steps)
//             progressBar.value = (int)completedStep;
//         }
//     }

//     // You might want a method to load calibration data in your main game scene
//     public static float GetMinPotValue() { return PlayerPrefs.GetFloat(MinPotKey, 0f); }
//     public static float GetMaxPotValue() { return PlayerPrefs.GetFloat(MaxPotKey, 1023f); } // Use a reasonable default
//     public static float GetMinFsrValue() { return PlayerPrefs.GetFloat(MinFsrKey, 0f); } // Added GetMinFsrValue
//     public static float GetMaxFsrValue() { return PlayerPrefs.GetFloat(MaxFsrKey, 1023f); } // Use a reasonable default
//     public static float GetMinTofValue() { return PlayerPrefs.GetFloat(MinTofKey, 0f); } // Added GetMinTofValue
//     public static float GetMaxTofValue() { return PlayerPrefs.GetFloat(MaxTofKey, 70f); } // Use a reasonable default (based on VL6180X range)
// }

using UnityEngine;
using UnityEngine.UI; // Required for Slider/Image for progress bar and Button
using TMPro; // Required for TextMeshProUGUI
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for List (if averaging)
using UnityEngine.SceneManagement; // Required for SceneManager
using System.Linq; // Required for LINQ operations

public class CalibrationManagerScript : MonoBehaviour
{
    // --- UI References ---
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI rawValuesText; // Optional
    public Slider progressBar; // Or public Image progressBarFill;
    public Button readyButton; // <<< ADDED: Assign your "Ready" button in the Inspector
    // public TextMeshProUGUI readyButtonText; // <<< ADDED: Assign the TextMeshProUGUI on your Ready button
    public Image instructionImage; // <<< ADDED: Assign an Image component to display instruction pictures
    private BLEManager bleManager; // Reference to your BLE Manager takes the instance of the BLEManager from main menu (singleton)

    // --- Calibration Settings ---
    public float dataCollectionDuration = 15.0f; // How long to collect data for each step
    public string nextSceneName = "MainGameScene"; // Name of the scene to load after calibration

    // --- Instruction Sprites ---
    // Assign sprites for each calibration step in the Inspector
    public Sprite potLeftSprite;
    public Sprite potRightSprite;
    public Sprite fingerRestSprite;
    public Sprite fingerExtendSprite;
    public Sprite completeSprite; // Optional sprite for completion

    // --- Private Variables ---
    // Updated enum to reflect the process states
    private enum CalibrationState { NotStarted, WaitingForReady, CollectingData, ProcessingData, Complete, Saving, Error }
    private CalibrationState currentState = CalibrationState.NotStarted;

    // Enum to represent the specific calibration steps (used for processing and progress)
    private enum CalibrationStep { None, PotLeft, PotRight, FingerRest ,FingerExtend }


    private List<float> potValues = new List<float>();
    private List<float> fsrValues = new List<float>();
    private List<float> tofValues = new List<float>();

    private bool isReady = false; // Flag to track when the user is ready for the next step

    // --- PlayerPrefs Keys ---
    private const string MinPotKey = "MinPotValue";
    private const string MaxPotKey = "MaxPotValue";
    private const string MinFsrKey = "MinFsrValue";
    private const string MaxFsrKey = "MaxFsrValue";
    private const string MinTofKey = "MinTofValue";
    private const string MaxTofKey = "MaxTofValue";

    void Start()
    {
        // Get the BLEManager instance
        bleManager = BLEManager.Instance;
        if (bleManager == null)
        {
            Debug.LogError("BLEManager instance not found! Ensure it's in the scene and set as a Singleton.");
            SetState(CalibrationState.Error, "BLE Manager not found. Cannot calibrate.");
            // Disable button if BLE Manager is missing
            if (readyButton != null) readyButton.gameObject.SetActive(false);
            return; // Stop if BLEManager is missing
        }

        // Initialize UI and start calibration flow
        if (progressBar != null)
        {
            // Set max value to the number of distinct calibration steps (PotLeft, PotRight, FingerRest, FingerExtend)
            progressBar.minValue = 0;
            progressBar.maxValue = 4;
            progressBar.value = 0;
        }

        // Hide the ready button initially
        if (readyButton != null) readyButton.gameObject.SetActive(false);
        // Hide the instruction image initially
        if (instructionImage != null) instructionImage.gameObject.SetActive(false);


        SetState(CalibrationState.NotStarted, "Please ensure your device is connected.");
        StartCoroutine(StartCalibrationFlow());
    }

    void Update()
    {
        // Optional: Update raw values display
        if (rawValuesText != null && bleManager != null && bleManager.isConnected)
        {
            rawValuesText.text = $"Pot: {bleManager.latestPotValue:F2}, FSR: {bleManager.latestFsrValue:F2}, ToF: {bleManager.latestTofValue:F2}";
        }
        else if (rawValuesText != null)
        {
            rawValuesText.text = "Waiting for BLE Connection...";
        }
    }

    // --- Public method called by the Ready button ---
    public void OnReadyButtonClick()
    {
        isReady = true; // Set the flag when the button is clicked
        // Optionally disable the button immediately after click
        if (readyButton != null) readyButton.interactable = false;
    }

    // --- Calibration Flow Coroutine ---
    IEnumerator StartCalibrationFlow()
    {
        // Give a moment for connection/start
        yield return new WaitForSeconds(1.0f); // Adjust delay as needed

        // Wait until connected before starting steps
        while (bleManager == null || !bleManager.isConnected)
        {
             SetState(CalibrationState.NotStarted, "Connecting to device...");
             yield return null; // Wait a frame
        }

        // Initial welcome message and ready button
        yield return ShowInstructionAndAwaitReady("Welcome to Calibration!\nThis process will guide you through setting up your controller.\nClick Ready to begin.", null); // No specific image for welcome


        // --- Step 1: Potentiometer Left ---
        yield return ShowInstructionAndAwaitReady("Step 1/4: Rotate potentiometer fully LEFT and click Ready.", potLeftSprite);
        yield return CollectDataForStep(CalibrationStep.PotLeft, "Hold potentiometer fully LEFT..."); // Pass the specific step

        // --- Step 2: Potentiometer Right ---
        yield return ShowInstructionAndAwaitReady("Step 2/4: Rotate potentiometer fully RIGHT and click Ready.", potRightSprite);
        yield return CollectDataForStep(CalibrationStep.PotRight, "Hold potentiometer fully RIGHT..."); // Pass the specific step

        // --- Step 3: Finger Rest ---
        yield return ShowInstructionAndAwaitReady("Step 3/4: Relax your fingers fully (or remove hand) and click Ready.", fingerRestSprite);
        yield return CollectDataForStep(CalibrationStep.FingerRest, "Hold fingers relaxed..."); // Pass the specific step

        // --- Step 4: Finger Extend ---
        yield return ShowInstructionAndAwaitReady("Step 4/4: EXTEND your fingers fully and click Ready.", fingerExtendSprite);
        yield return CollectDataForStep(CalibrationStep.FingerExtend, "Hold fingers extended..."); // Pass the specific step

        // --- Calibration Complete ---
        SetState(CalibrationState.Complete, "Calibration Complete!");
        if (instructionImage != null && completeSprite != null) instructionImage.sprite = completeSprite;
        SaveCalibrationData();

        // Hide the button and progress bar on completion
        if (readyButton != null) readyButton.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (rawValuesText != null) rawValuesText.gameObject.SetActive(false); // Hide raw values on completion


        yield return new WaitForSeconds(3.0f); // Show completion message for a bit

        // Load the next scene
        SceneManager.LoadSceneAsync(nextSceneName);
    }

    // --- Coroutine to show instruction, display button, and wait for click ---
    IEnumerator ShowInstructionAndAwaitReady(string instruction, Sprite instructionSprite)
    {
        SetState(CalibrationState.WaitingForReady, instruction);

        // Display the instruction image if provided
        if (instructionImage != null)
        {
            if (instructionSprite != null)
            {
                instructionImage.gameObject.SetActive(true);
                instructionImage.sprite = instructionSprite;
            }
            else
            {
                 instructionImage.gameObject.SetActive(false); // Hide image if no sprite provided
            }
        }

        // Show and enable the ready button
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
            readyButton.interactable = true;
            // if (readyButtonText != null) readyButtonText.text = "Ready!"; // Set button text
        }

        isReady = false; // Reset the flag

        // Wait until the ready button is clicked
        while (!isReady)
        {
            yield return null; // Wait a frame
        }

        // Hide the instruction image and button after ready is clicked
        if (instructionImage != null) instructionImage.gameObject.SetActive(false);
        if (readyButton != null) readyButton.gameObject.SetActive(false);

        // Optional: Display a brief message indicating data collection is starting
        // SetState(CalibrationState.CollectingData, $"Collecting data for {dataCollectionDuration.ToString("F1")} seconds...");

        // Give a very short delay before starting data collection to allow UI to update
        yield return new WaitForSeconds(0.5f);
    }


    // --- Data Collection Coroutine for a single step ---
    // Modified to accept the specific CalibrationStep
    IEnumerator CollectDataForStep(CalibrationStep step, string holdInstruction)
    {
        SetState(CalibrationState.CollectingData, holdInstruction); // Set state and instruction for holding

        // Ensure raw values text is visible during data collection
        if (rawValuesText != null) rawValuesText.gameObject.SetActive(true);

        // Clear previous data
        potValues.Clear();
        fsrValues.Clear();
        tofValues.Clear();

        float timer = 0f;
        while (timer < dataCollectionDuration)
        {
            // Collect current values
            if (bleManager != null && bleManager.isConnected) // Only collect if BLE is connected
            {
                potValues.Add(bleManager.latestPotValue);
                fsrValues.Add(bleManager.latestFsrValue);
                tofValues.Add(bleManager.latestTofValue);
            }


            timer += Time.deltaTime;
            // Update progress bar and countdown
            if (progressBar != null)
            {
                 // Calculate progress within the current step (0 to 1)
                 float stepProgress = timer / dataCollectionDuration;
                 // Map step progress to overall progress bar value based on the step number
                 float overallProgress = 0;
                 switch (step)
                 {
                     case CalibrationStep.PotLeft: overallProgress = 0 + stepProgress; break;
                     case CalibrationStep.PotRight: overallProgress = 1 + stepProgress; break;
                     case CalibrationStep.FingerRest: overallProgress = 2 + stepProgress; break;
                     case CalibrationStep.FingerExtend: overallProgress = 3 + stepProgress; break;
                 }
                 progressBar.value = overallProgress;
            }
            if (instructionText != null && currentState == CalibrationState.CollectingData)
            {
                 instructionText.text = $"{holdInstruction} { (dataCollectionDuration - timer).ToString("F1") } seconds remaining.";
            }

            yield return null; // Wait for the next frame
        }

        // Process collected data based on the step
        SetState(CalibrationState.ProcessingData, "Processing data...");
        // Hide raw values text during processing
        if (rawValuesText != null) rawValuesText.gameObject.SetActive(false);

        ProcessCollectedData(step); // Pass the specific step to processing

        // Update progress bar to show step completion
        UpdateProgressBar(step); // Pass the specific step to update progress bar

        // Small delay after processing before showing the next instruction
        yield return new WaitForSeconds(0.5f);
    }

    // --- Process data after collection for a step ---
    // Modified to accept the specific CalibrationStep
    void ProcessCollectedData(CalibrationStep step)
    {
        switch (step)
        {
            case CalibrationStep.PotLeft: // Use CalibrationStep enum here
                // Calculate the average of collected potentiometer values
                if (potValues.Count > 0)
                {
                    float averageMinPot = potValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MinPotKey, averageMinPot);
                    Debug.Log($"Calibrated Min Pot (Average): {averageMinPot}");
                }
                else
                {
                     Debug.LogWarning("No potentiometer data collected for PotLeft step.");
                }
                break;

            case CalibrationStep.PotRight: // Use CalibrationStep enum here
                 // Calculate the average of collected potentiometer values
                if (potValues.Count > 0)
                {
                    float averageMaxPot = potValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MaxPotKey, averageMaxPot);
                    Debug.Log($"Calibrated Max Pot (Average): {averageMaxPot}");
                }
                 else
                {
                     Debug.LogWarning("No potentiometer data collected for PotRight step.");
                }
                break;
            case CalibrationStep.FingerRest: // Use CalibrationStep enum here
                // Calculate the average of collected FSR and TOF values for the 'closed' state
                if (fsrValues.Count > 0)
                {
                    float averageMinFsr = fsrValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MinFsrKey, averageMinFsr);
                    Debug.Log($"Calibrated Min FSR (Average): {averageMinFsr}");
                }
                else
                {
                     Debug.LogWarning("No FSR data collected for FingerRest step.");
                }
                if (tofValues.Count > 0)
                {
                    float averageMinTof = tofValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MinTofKey, averageMinTof); // Store as MinTofKey for relaxed/closed
                    Debug.Log($"Calibrated Min ToF (Average): {averageMinTof}");
                }
                 else
                {
                     Debug.LogWarning("No TOF data collected for FingerRest step.");
                }
                break;

            case CalibrationStep.FingerExtend: // Use CalibrationStep enum here
                // Calculate the average of collected FSR and TOF values for the 'open' state
                if (fsrValues.Count > 0)
                {
                    float averageMaxFsr = fsrValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MaxFsrKey, averageMaxFsr);
                    Debug.Log($"Calibrated Max FSR (Average): {averageMaxFsr}");
                }
                 else
                {
                     Debug.LogWarning("No FSR data collected for FingerExtend step.");
                }
                 if (tofValues.Count > 0)
                {
                    float averageMaxTof = tofValues.Average(); // Use LINQ Average()
                    PlayerPrefs.SetFloat(MaxTofKey, averageMaxTof); // Store as MaxTofKey for extended/open
                    Debug.Log($"Calibrated Max ToF (Average): {averageMaxTof}");
                }
                 else
                {
                     Debug.LogWarning("No TOF data collected for FingerExtend step.");
                }
                break;
        }
    }
    // --- Save all PlayerPrefs (optional, can save after each step or at the end) ---
    void SaveCalibrationData()
    {
        PlayerPrefs.Save(); // Ensure data is written to disk
        Debug.Log("Calibration data saved to PlayerPrefs.");
    }

    // --- Update UI State and Instruction Text ---
    void SetState(CalibrationState newState, string instruction)
    {
        currentState = newState;
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
        Debug.Log($"Calibration State: {currentState} - {instruction}");

        // You could also enable/disable different UI elements based on the state here
        // Example: Hide raw values text during instruction/waiting states
        if (rawValuesText != null)
        {
             rawValuesText.gameObject.SetActive(currentState == CalibrationState.CollectingData || currentState == CalibrationState.ProcessingData);
        }
    }

    // --- Update Progress Bar ---
    // Modified to accept the specific CalibrationStep that was completed
    void UpdateProgressBar(CalibrationStep completedStep)
    {
        if (progressBar != null)
        {
            // Map the completed step to the progress bar value
            switch (completedStep)
            {
                 case CalibrationStep.PotLeft:
                     progressBar.value = 1;
                     break;
                 case CalibrationStep.PotRight:
                     progressBar.value = 2;
                     break;
                 case CalibrationStep.FingerRest:
                     progressBar.value = 3;
                     break;
                 case CalibrationStep.FingerExtend:
                     progressBar.value = 4; // All 4 steps completed
                     break;
                 // No default case needed for completed steps, as it's called after a specific step finishes
            }
        }
    }

    // You might want a method to load calibration data in your main game scene
    public static float GetMinPotValue() { return PlayerPrefs.GetFloat(MinPotKey, 0f); }
    public static float GetMaxPotValue() { return PlayerPrefs.GetFloat(MaxPotKey, 1023f); } // Use a reasonable default
    public static float GetMinFsrValue() { return PlayerPrefs.GetFloat(MinFsrKey, 0f); } // Added GetMinFsrValue
    public static float GetMaxFsrValue() { return PlayerPrefs.GetFloat(MaxFsrKey, 1023f); } // Use a reasonable default
    public static float GetMinTofValue() { return PlayerPrefs.GetFloat(MinTofKey, 0f); } // Added GetMinTofValue
    public static float GetMaxTofValue() { return PlayerPrefs.GetFloat(MaxTofKey, 70f); } // Use a reasonable default (based on VL6180X range)
}