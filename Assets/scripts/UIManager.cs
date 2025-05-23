// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class UIManager : MonoBehaviour
// {
//     public TextMeshProUGUI connectionStatus;
//     public TextMeshProUGUI messageText;
//     public TextMeshProUGUI debugText; // assign in Inspector

//     public void UpdateConnectionStatus(bool connected)
//     {
//         connectionStatus.text = connected ? "Connected" : "Disconnected";
//     }

//     public void UpdateMessage(string msg)
//     {
//         messageText.text = msg;
//     }

//     public void UpdateDebug(string debugMsg)
//     {
//         if (debugText != null)
//             debugText.text = debugMsg;
//     }
// }



// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class UIManager : MonoBehaviour
// {
//     // Singleton instance
//     public static UIManager Instance;

//     // UI Fields for Connection Status and General Messages
//     public TextMeshProUGUI connectionStatus;
//     public TextMeshProUGUI messageText;

//     // UI Fields for Debugging Sensor Data and Mapping
//     public TextMeshProUGUI debugText; // General debug text field
//     public TextMeshProUGUI rawPotText;
//     public TextMeshProUGUI rawFsrText;
//     public TextMeshProUGUI rawTofText;
//     public TextMeshProUGUI calibratedPotRangeText;
//     // Added fields for FSR and ToF calibrated ranges
//     public TextMeshProUGUI calibratedFsrRangeText;
//     public TextMeshProUGUI calibratedTofRangeText;

//     public TextMeshProUGUI mappedPotValueText; // Display the calculated target X position or normalized value
//     // Updated mapped value text to reflect combined FSR/ToF
//     public TextMeshProUGUI mappedMouthValueText; // Display the normalized combined FSR/ToF value (0-1)

//     public TextMeshProUGUI currentMouthStateText; // Display the current MouthState enum value

//     void Awake()
//     {
//         // Implement the Singleton pattern and make it persistent
//         if (Instance == null)
//         {
//             Instance = this;
//             // Keep the UIManager alive across scenes
//             DontDestroyOnLoad(gameObject);
//             Debug.Log("UIManager instance created and set to DontDestroyOnLoad.");
//         }
//         else if (Instance != this) // If an instance already exists and it's not this one
//         {
//             // If a duplicate instance is found (from another scene), destroy it
//             Debug.LogWarning("Duplicate UIManager instance found, destroying this one.");
//             Destroy(gameObject);
//         }
//         // If Instance == this, it means this is the existing, persistent instance.
//     }


//     // --- Methods to update UI fields ---

//     public void UpdateConnectionStatus(bool connected)
//     {
//         if (connectionStatus != null)
//         {
//             connectionStatus.text = connected ? "Connected" : "Disconnected";
//         }
//     }

//     public void UpdateMessage(string msg)
//     {
//         if (messageText != null)
//         {
//             messageText.text = msg;
//         }
//     }

//     public void UpdateDebug(string debugMsg)
//     {
//         if (debugText != null)
//         {
//             debugText.text = debugMsg;
//         }
//     }

//     // New methods to update specific debug fields
//     public void UpdateRawValues(float pot, float fsr, float tof)
//     {
//         if (rawPotText != null) rawPotText.text = $"Raw Pot: {pot:F2}";
//         if (rawFsrText != null) rawFsrText.text = $"Raw FSR: {fsr:F2}";
//         if (rawTofText != null) rawTofText.text = $"Raw ToF: {tof:F2}";
//     }

//     // Updated method to include FSR and ToF ranges
//     public void UpdateCalibratedRanges(float minPot, float maxPot, float minFsr, float maxFsr, float minTof, float maxTof)
//     {
//         if (calibratedPotRangeText != null) calibratedPotRangeText.text = $"Cal Pot: {minPot:F2}-{maxPot:F2}";
//         if (calibratedFsrRangeText != null) calibratedFsrRangeText.text = $"Cal FSR: {minFsr:F2}-{maxFsr:F2}";
//         if (calibratedTofRangeText != null) calibratedTofRangeText.text = $"Cal ToF: {minTof:F2}-{maxTof:F2}";
//     }

//     // Updated method to reflect combined mouth mapping
//     public void UpdateMappedValues(float mappedPotX, float mappedMouth)
//     {
//         if (mappedPotValueText != null) mappedPotValueText.text = $"Mapped Pot (X): {mappedPotX:F2}"; // Target X position
//         if (mappedMouthValueText != null) mappedMouthValueText.text = $"Mapped Mouth (0-1): {mappedMouth:F2}"; // Normalized combined value
//     }

//     public void UpdateMouthState(string state)
//     {
//         if (currentMouthStateText != null) currentMouthStateText.text = $"Mouth State: {state}";
//     }
// }

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for SceneManager

public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance;

    // Reference to the Canvas containing the debug UI
    private Canvas debugCanvas;

    // UI Fields for Connection Status and General Messages
    public TextMeshProUGUI connectionStatus;
    public TextMeshProUGUI messageText;

    // UI Fields for Debugging Sensor Data and Mapping
    public TextMeshProUGUI debugText; // General debug text field
    public TextMeshProUGUI rawPotText;
    public TextMeshProUGUI rawFsrText;
    public TextMeshProUGUI rawTofText;
    public TextMeshProUGUI calibratedPotRangeText;
    // Added fields for FSR and ToF calibrated ranges
    public TextMeshProUGUI calibratedFsrRangeText;
    public TextMeshProUGUI calibratedTofRangeText;

    public TextMeshProUGUI mappedPotValueText; // Display the calculated target X position or normalized value
    // Updated mapped value text to reflect combined FSR/ToF
    public TextMeshProUGUI mappedMouthValueText; // Display the normalized combined FSR/ToF value (0-1)

    public TextMeshProUGUI currentMouthStateText; // Display the current MouthState enum value

    public GameObject[] debugUIElements; // Assign all debug TextMeshProUGUI GameObjects here in Inspector

    void Awake()
    {
        // Implement the Singleton pattern and make it persistent
        if (Instance == null)
        {
            Instance = this;
            // Keep the UIManager alive across scenes
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager: Awake - Instance created and set to DontDestroyOnLoad.");
        }
        else if (Instance != this) // If an instance already exists and it's not this one
        {
            // If a duplicate instance is found (from another scene), destroy it
            Debug.LogWarning("UIManager: Awake - Duplicate instance found, destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("UIManager: Awake - Existing instance found.");
        }

        // Get the Canvas component that is a child of this GameObject
        debugCanvas = GetComponentInChildren<Canvas>(true); // Include inactive GameObjects
        if (debugCanvas == null)
        {
            Debug.LogError("UIManager: Awake - Could not find a Canvas component as a child!");
        }
    }

    void OnEnable()
    {
        Debug.Log("UIManager: OnEnable");
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Ensure the debug Canvas is active when this script is enabled
        if (debugCanvas != null)
        {
            debugCanvas.gameObject.SetActive(true);
            Debug.Log("UIManager: OnEnable - Debug Canvas set to Active.");
        }
    }

    void OnDisable()
    {
        Debug.Log("UIManager: OnDisable");
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // Optional: You might want to keep the debug Canvas active even if the UIManager script is disabled
        // If you want it to hide when the script is disabled, uncomment the line below:
        // if (debugCanvas != null) debugCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        Debug.Log("UIManager: Start");
        // Ensure the debug Canvas is active when the script starts
        if (debugCanvas != null)
        {
            debugCanvas.gameObject.SetActive(true);
            Debug.Log("UIManager: Start - Debug Canvas set to Active.");
        }
        // Initial UI update (optional, BLEManager might update on connect/data)
        UpdateConnectionStatus(false); // Assume disconnected initially
        UpdateMessage("Waiting for BLE connection...");
        UpdateRawValues(0, 0, 0);
        UpdateCalibratedRanges(0, 0, 0, 0, 0, 0);
        UpdateMappedValues(0, 0);
        UpdateMouthState("Closed (No Data)");
    }

    // This method is called automatically when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"UIManager: OnSceneLoaded - Scene loaded: {scene.name}");
        // Ensure the debug Canvas is active every time a new scene is loaded
        if (debugCanvas != null)
        {
            debugCanvas.gameObject.SetActive(true);
            Debug.Log($"UIManager: OnSceneLoaded - Debug Canvas set to Active for scene: {scene.name}");
        }
        // You might want to update the UI message based on the scene here
        UpdateMessage($"Scene Loaded: {scene.name}");
    }

    void Update()
    {
        // Optional: Add a debug log here to confirm Update is running
        // Debug.Log("UIManager: Update is running.");
    }


    // --- Methods to update UI fields ---

    public void UpdateConnectionStatus(bool connected)
    {
        if (connectionStatus != null)
        {
            connectionStatus.text = connected ? "Connected" : "Disconnected";
        }
    }

    public void UpdateMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
        }
    }

    public void UpdateDebug(string debugMsg)
    {
        if (debugText != null)
        {
            debugText.text = debugMsg;
        }
    }

    // New methods to update specific debug fields
    public void UpdateRawValues(float pot, float fsr, float tof)
    {
        if (rawPotText != null) rawPotText.text = $"Raw Pot: {pot:F2}";
        if (rawFsrText != null) rawFsrText.text = $"Raw FSR: {fsr:F2}";
        if (rawTofText != null) rawTofText.text = $"Raw ToF: {tof:F2}";
    }

    // Updated method to include FSR and ToF ranges
    public void UpdateCalibratedRanges(float minPot, float maxPot, float minFsr, float maxFsr, float minTof, float maxTof)
    {
        if (calibratedPotRangeText != null) calibratedPotRangeText.text = $"Cal Pot: {minPot:F2}-{maxPot:F2}";
        if (calibratedFsrRangeText != null) calibratedFsrRangeText.text = $"Cal FSR: {minFsr:F2}-{maxFsr:F2}";
        if (calibratedTofRangeText != null) calibratedTofRangeText.text = $"Cal ToF: {minTof:F2}-{maxTof:F2}";
    }

    // Updated method to reflect combined mouth mapping
    public void UpdateMappedValues(float mappedPotX, float mappedMouth)
    {
        if (mappedPotValueText != null) mappedPotValueText.text = $"Mapped Pot (X): {mappedPotX:F2}"; // Target X position
        if (mappedMouthValueText != null) mappedMouthValueText.text = $"Mapped Mouth (0-1): {mappedMouth:F2}"; // Normalized combined value
    }

    public void UpdateMouthState(string state)
    {
        if (currentMouthStateText != null) currentMouthStateText.text = $"Mouth State: {state}";
    }
    

    
}