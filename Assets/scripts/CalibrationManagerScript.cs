using UnityEngine;
using UnityEngine.UI; // Required for Slider/Image for progress bar
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

    // --- Calibration Settings ---
    public float dataCollectionDuration = 15.0f; // How long to collect data for each step
    public string nextSceneName = "MainGameScene"; // Name of the scene to load after calibration

    // --- Private Variables ---
    private BLEManager bleManager; // Reference to your BLE Manager takes the instance of the BLEManager from main menu (singleton)
    private enum CalibrationState { NotStarted, PotLeft, PotRight, FingerRest ,FingerExtend, Complete, Saving, Error }
    private CalibrationState currentState = CalibrationState.NotStarted;

    private List<float> potValues = new List<float>();
    private List<float> fsrValues = new List<float>();
    private List<float> tofValues = new List<float>();

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
            return; // Stop if BLEManager is missing
        }

        // Initialize UI and start calibration flow
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = 5; // 
            progressBar.value = 0;
        }

        SetState(CalibrationState.NotStarted, "Please ensure your device is connected. Starting calibration shortly...");
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

    // --- Calibration Flow Coroutine ---
    IEnumerator StartCalibrationFlow()
    {
        // Give a moment for connection/start
        yield return new WaitForSeconds(2.0f); // Adjust delay as needed

        // Wait until connected before starting steps
        while (bleManager == null || !bleManager.isConnected)
        {
             SetState(CalibrationState.NotStarted, "Connecting to device...");
             yield return null; // Wait a frame
        }

        SetState(CalibrationState.PotLeft, "Step 1/4: Rotate potentiometer fully LEFT and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
        yield return CollectDataForStep(CalibrationState.PotLeft);

        SetState(CalibrationState.PotRight, "Step 2/4: Rotate potentiometer fully RIGHT and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
        yield return CollectDataForStep(CalibrationState.PotRight);

        SetState(CalibrationState.FingerRest, "Step 3/4: Relax your fingers fully and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds."); // Clarified instruction
        yield return CollectDataForStep(CalibrationState.FingerRest);
        
        SetState(CalibrationState.FingerExtend, "Step 4/4: EXTEND your fingers fully and HOLD it there for " + dataCollectionDuration.ToString("F1") + " seconds.");
        yield return CollectDataForStep(CalibrationState.FingerExtend);

        SetState(CalibrationState.Complete, "Calibration Complete!");
        SaveCalibrationData();

        yield return new WaitForSeconds(10.0f); // Show completion message for a bit

        // or load tutorial scene here???
        SceneManager.LoadSceneAsync(1); //main game
    }

    // --- Data Collection Coroutine for a single step ---
    IEnumerator CollectDataForStep(CalibrationState step)
    {
        // Clear previous data
        potValues.Clear();
        fsrValues.Clear();
        tofValues.Clear();

        float timer = 0f;
        while (timer < dataCollectionDuration)
        {
            // Collect current values
            potValues.Add(bleManager.latestPotValue);
            fsrValues.Add(bleManager.latestFsrValue);
            tofValues.Add(bleManager.latestTofValue);

            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Process collected data based on the step
        ProcessCollectedData(step);

        // Update progress bar
        UpdateProgressBar(step);
    }

    // --- Process data after collection for a step ---
    
    void ProcessCollectedData(CalibrationState step)
    {
        switch (step)
        {
            case CalibrationState.PotLeft:
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

            case CalibrationState.PotRight:
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
            case CalibrationState.FingerRest:
                // Calculate the average of collected FSR and TOF values
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
                    PlayerPrefs.SetFloat(MinTofKey, averageMinTof);
                    Debug.Log($"Calibrated Max ToF (Average): {averageMinTof}");
                }
                 else
                {
                     Debug.LogWarning("No TOF data collected for FingerRest step.");
                }
                break;

            case CalibrationState.FingerExtend:
                // Calculate the average of collected FSR and TOF values
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
                    PlayerPrefs.SetFloat(MaxTofKey, averageMaxTof);
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
    }

    // --- Update Progress Bar ---
    void UpdateProgressBar(CalibrationState completedStep)
    {
        if (progressBar != null)
        {
            // Map enum value to progress bar value (assuming enum order matches steps)
            progressBar.value = (int)completedStep;
    }

    // You might want a method to load calibration data in your main game scene
    public static float GetMinPotValue() { return PlayerPrefs.GetFloat(MinPotKey, 0f); }
    public static float GetMaxPotValue() { return PlayerPrefs.GetFloat(MaxPotKey, 1023f); } // Use a reasonable default
    public static float GetMinFsrValue() { return PlayerPrefs.GetFloat(MinFsrKey, 0f); } // Added GetMinFsrValue
    public static float GetMaxFsrValue() { return PlayerPrefs.GetFloat(MaxFsrKey, 1023f); } // Use a reasonable default
    public static float GetMinTofValue() { return PlayerPrefs.GetFloat(MinTofKey, 0f); } // Added GetMinTofValue
    public static float GetMaxTofValue() { return PlayerPrefs.GetFloat(MaxTofKey, 70f); } // Use a reasonable default (based on VL6180X range)
}