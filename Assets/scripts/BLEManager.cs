
// using System;
// using TMPro; // Assuming UIManager still uses TextMeshProUGUI
// using UnityEngine;


// public class BLEManager : MonoBehaviour
// {
//     // Singleton instance to easily access this manager from other scripts
//     public static BLEManager Instance;

//     public string serviceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
//     public string characteristicUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";
//     public string deviceName = "ReCover";

//     // Reference to a UIManager. This might be assigned in the first scene,
//     // or found dynamically in subsequent scenes if needed for UI updates there.
//     public UIManager uiManager;

//     // Public variables to store the latest received sensor data
//     public float latestFsrValue { get; private set; }
//     public float latestPotValue { get; private set; }
//     public float latestTofValue { get; private set; }

//     public bool isConnected { get; private set; }

//     // Store the address of the connected device
//     private string connectedDeviceAddress;

//     void Awake()
//     {
//         // Implement the Singleton pattern
//         if (Instance == null)
//         {
//             Instance = this;
//             // Keep the object alive across scenes
//             DontDestroyOnLoad(gameObject);
//             Debug.Log("BLEManager instance created and set to DontDestroyOnLoad.");
//         }
//         else if (Instance != this) // If an instance already exists and it's not this one
//         {
//             // If a duplicate instance is found (from another scene), destroy it
//             Debug.LogWarning("Duplicate BLEManager instance found, destroying this one.");
//             Destroy(gameObject);
//         }
//         // If Instance == this, it means this is the existing, persistent instance.
//     }

//     void Start()
//     {
//         // Attempt to find the UIManager in the current scene if it's not assigned
//         if (uiManager == null)
//         {
//              uiManager = FindObjectOfType<UIManager>();
//              if (uiManager != null)
//              {
//                  Debug.Log("BLEManager found UIManager in the current scene.");
//              }
//         }

//         // Initialize BLE interface
//         BluetoothLEHardwareInterface.Initialize(true, false, () =>
//         {
//             // Initialization successful callback
//             isConnected = false; // Set connected status to false initially
//             if (uiManager != null)
//             {
//                 uiManager.UpdateConnectionStatus(false); // Update UI
//                 uiManager.UpdateMessage("BLE Initialized. Scanning..."); // Initial message
//             }
//              else
//             {
//                  Debug.LogWarning("UIManager not assigned/found in BLEManager for initial UI update.");
//             }

//             // Start scanning for peripherals
//             ScanForDevice();

//         },
//         // Error callback during initialization
//         (error) =>
//         {
//             Debug.LogError("BLE Initialization Error: " + error);
//             isConnected = false; // Set connected status to false on error
//             if (uiManager != null)
//             {
//                 uiManager.UpdateConnectionStatus(isConnected); // Update UI
//                 uiManager.UpdateMessage("BLE Init Error: " + error); // Show error message
//             }
//         });
//     }

//     // Update is not typically needed for continuous data reception with SubscribeCharacteristic

//     void ScanForDevice()
//     {
//          Debug.Log("Scanning for device: " + deviceName);
//          if (uiManager != null)
//          {
//              uiManager.UpdateMessage("Scanning..."); // Update UI message
//          }

//         // Scan for peripherals using the 4-argument overload from the original script
//         // The second callback takes only address and name (Action<string, string>).
//         BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
//             null, // Service UUIDs (string[]) - 1st arg
//             // Found peripheral callback (Signature from original commented-out script) - 2nd arg
//             (address, name) => // Action<string, string>
//             {
//                 Debug.Log($"Found device: {name} at address: {address}");
//                 if (name.Contains(deviceName)) // Check if the device name matches
//                 {
//                     Debug.Log($"Found target device: {deviceName}. Stopping scan.");
//                     BluetoothLEHardwareInterface.StopScan(); // Stop scanning once found
//                     ConnectToDevice(address); // Attempt to connect
//                 }
//             },
//             // Scan stopped callback (optional) - 3rd arg
//             null, // Action<string> or null
//             // Allow duplicates (bool) - 4th arg
//             false); // bool
//     }


//     void ConnectToDevice(string address)
//     {
//         Debug.Log("Attempting to connect to " + address);
//          if (uiManager != null)
//          {
//              uiManager.UpdateMessage("Connecting..."); // Update UI message
//          }

//         // Connect to the peripheral using the most likely 5-argument overload we identified.
//         // The 4th callback confirms connection and might provide initial discovery details (Action<string, string, string>).
//         // The 5th callback is for errors (Action<string>), potentially including disconnection.
//         BluetoothLEHardwareInterface.ConnectToPeripheral(
//             address,
//             null, // Service Found Callback (unused in this flow) - Action<string, string> or null
//             null, // Characteristic Found Callback (unused in this flow) - Action<string, string, string> or null
//             // Connection Success / Initial Discovery Callback (Signature from our previous attempt based on StartingExample) - 4th arg
//             (addressCallback, serviceUUIDCallback, characteristicUUIDCallback) => // Action<string, string, string>
//             {
//                 // This callback should fire upon successful connection.
//                 // It might also be called for each service/characteristic discovered.
//                 // We set connected status and subscribe the first time it confirms connection.
//                 if (!isConnected)
//                 {
//                      Debug.Log($"Connected to {addressCallback}. Initial Discovery - Service: {serviceUUIDCallback}, Char: {characteristicUUIDCallback}");
//                     isConnected = true; // Set connected status to true
//                     connectedDeviceAddress = addressCallback; // Store the connected address

//                     if (uiManager != null)
//                     {
//                         uiManager.UpdateConnectionStatus(isConnected); // Update UI
//                         uiManager.UpdateMessage("Connected. Subscribing..."); // Update UI message
//                     }

//                     // Proceed to subscribing after connection is confirmed.
//                     // Data reception will happen via the SubscribeCharacteristicWithDeviceAddress callback.
//                     BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
//                         connectedDeviceAddress, // Use the stored connected address
//                         this.serviceUUID, // Use the class member serviceUUID
//                         this.characteristicUUID, // Use the class member characteristicUUID
//                         // Subscribe success callback (optional)
//                         null, // Action<string, string> or null
//                         // Data received callback - This callback is where the sensor data (bytes) arrives
//                         (addressData, characteristicUUIDData, bytes) => // Signature observed in StartingExample.cs Subscribe
//                         {
//                             // Data received from the characteristic
//                             ProcessReceivedData(bytes); // Process the raw bytes
//                         });
//                 }
//                 // If you needed to handle discovery of other specific services/characteristics,
//                 // you would add logic here using serviceUUIDCallback and characteristicUUIDCallback.
//             },
//             // Error callback (Signature from our previous attempt based on original code/errors) - 5th arg
//             (error) => // Action<string>
//             {
//                 Debug.LogError("BLE Connection or Error: " + error);
//                 // This callback might also signal disconnection or other errors.
//                 // We'll assume it means disconnected or an error occurred.
//                 isConnected = false; // Set connected status to false on error/disconnect
//                 connectedDeviceAddress = null; // Clear connected address

//                 if (uiManager != null)
//                 {
//                     uiManager.UpdateConnectionStatus(isConnected); // Update UI (assuming error means disconnected)
//                     uiManager.UpdateMessage("BLE Error: " + error); // Show error message
//                 }
//                 // Optionally start scanning again after error
//                 ScanForDevice(); // Might need a delay or different handling for retries
//             });

//         // Note: With this 5-argument overload, there might not be a separate 'disconnected' callback parameter in ConnectToPeripheral.
//     }

//     // Process the raw byte data received from the SubscribeCharacteristicWithDeviceAddress callback
//     void ProcessReceivedData(byte[] bytes)
//     {
//          // Assuming the data is an array of 3 floats (Pot, FSR, ToF) sent as bytes
//          // Each float is 4 bytes, total 12 bytes.
//          // You mentioned Pot and FSR are 0-4095 and ToF is 0-70mm.
//          // Assuming they are sent as standard float (32-bit) converted to bytes.

//         if (bytes != null && bytes.Length >= 12) // 3 values * 4 bytes/value = 12 bytes minimum
//         {
//             // Ensure the order matches what your ESP32 is sending
//             // Assuming order is Pot (0-4095), FSR (0-4095), TOF (0-70mm) sent as floats
//              try
//              {
//                 latestPotValue = BitConverter.ToSingle(bytes, 0); // First 4 bytes for Pot
//                 latestFsrValue = BitConverter.ToSingle(bytes, 4); // Next 4 bytes for FSR
//                 latestTofValue = BitConverter.ToSingle(bytes, 8); // Last 4 bytes for ToF

//                 // Update UI message with the latest values (if UIManager is set up for this)
//                  if (uiManager != null && uiManager.messageText != null)
//                  {
//                       uiManager.UpdateMessage($"Pot: {latestPotValue:F2}, FSR: {latestFsrValue:F2}, ToF: {latestTofValue:F2}");
//                  }

//                 // You can also log the values to the console for debugging
//                 // Debug.Log($"Received Data - Pot: {latestPotValue}, FSR: {latestFsrValue}, ToF: {latestTofValue}");
//              }
//              catch (Exception ex)
//              {
//                  Debug.LogError($"Error processing received BLE data: {ex.Message}");
//                   if (uiManager != null && uiManager.debugText != null)
//                   {
//                      uiManager.UpdateDebug($"Data processing error: {ex.Message}");
//                   }
//              }
//         }
//         else
//         {
//             // This warning might occur if the subscription sends empty or unexpected data initially
//             Debug.LogWarning($"Received data bytes length ({bytes?.Length ?? 0}) is not as expected for 3 values (expected >= 12 bytes), or bytes is null.");
//              if (uiManager != null && uiManager.debugText != null)
//              {
//                   uiManager.UpdateDebug($"Received unexpected data format. Length: {bytes?.Length ?? 0}");
//              }
//         }
//     }

//     void OnApplicationQuit()
//     {
//         // Clean up BLE resources when the application quits
//          if (isConnected && !string.IsNullOrEmpty(connectedDeviceAddress))
//         {
//             // Attempt to disconnect the peripheral if the API supports it
//             // If DisconnectPeripheral(address, callback) is not available in your API,
//             // you might need to use DisconnectAll() or rely on DeInitialize.
//              try
//              {
//                  // This might also trigger a disconnected event/callback if one exists and is separate.
//                  BluetoothLEHardwareInterface.DisconnectPeripheral(connectedDeviceAddress, null);
//              }
//              catch (Exception ex)
//              {
//                  Debug.LogWarning($"Error attempting to disconnect peripheral: {ex.Message}. Relying on DeInitialize.");
//              }
//         }
//         // Give a moment for disconnect before deinitialization (if DisconnectPeripheral was successful)
//         // This is tricky in OnApplicationQuit, DeInitialize might be sufficient cleanup.

//         BluetoothLEHardwareInterface.DeInitialize(() => {
//             Debug.Log("BLE DeInitialized");
//         });
//     }

//     // --- Method to explicitly disconnect if needed ---
//     public void Disconnect()
//     {
//          if (isConnected && !string.IsNullOrEmpty(connectedDeviceAddress))
//         {
//              try
//              {
//                 BluetoothLEHardwareInterface.DisconnectPeripheral(connectedDeviceAddress, null);
//              }
//              catch (Exception ex)
//              {
//                 Debug.LogWarning($"Error attempting explicit disconnect: {ex.Message}.");
//              }
//         }
//     }
//     // -------------------------------------------------
// }
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager

public class BLEManager : MonoBehaviour
{
    // Singleton instance to easily access this manager from other scripts
    public static BLEManager Instance;

    public string serviceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
    public string characteristicUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";
    public string deviceName = "ReCover";

    // Reference to a UIManager. This will be updated when a new scene loads.
    public UIManager uiManager;

    // Public variables to store the latest received sensor data
    public float latestFsrValue { get; private set; }
    public float latestPotValue { get; private set; }
    public float latestTofValue { get; private set; }

    public bool isConnected { get; private set; }

    // Store the address of the connected device
    private string connectedDeviceAddress;

    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Keep the object alive across scenes
            DontDestroyOnLoad(gameObject);
            Debug.Log("BLEManager instance created and set to DontDestroyOnLoad.");
        }
        else if (Instance != this) // If an instance already exists and it's not this one
        {
            // If a duplicate instance is found (from another scene), destroy it
            Debug.LogWarning("Duplicate BLEManager instance found, destroying this one.");
            Destroy(gameObject);
        }
        // If Instance == this, it means this is the existing, persistent instance.
    }

    void OnEnable()
    {
        // Subscribe to the sceneLoaded event when this object becomes enabled
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("BLEManager subscribed to SceneManager.sceneLoaded.");
    }

    void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event when this object becomes disabled
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("BLEManager unsubscribed from SceneManager.sceneLoaded.");
    }

    // This method is called automatically when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}");
        // Find the UIManager in the newly loaded scene
        uiManager = FindObjectOfType<UIManager>();

        if (uiManager != null)
        {
            Debug.Log($"BLEManager found UIManager in scene: {scene.name}");
            // Optionally update the UI with current connection status and data on scene load
            uiManager.UpdateConnectionStatus(isConnected);
            if (isConnected)
            {
                 uiManager.UpdateMessage($"Connected. Pot: {latestPotValue:F2}, FSR: {latestFsrValue:F2}, ToF: {latestTofValue:F2}");
            }
            else
            {
                 uiManager.UpdateMessage("Disconnected. Scanning..."); // Or appropriate status
            }
             uiManager.UpdateDebug("UIManager reference updated on scene load.");
        }
        else
        {
            Debug.LogWarning($"BLEManager could not find UIManager in scene: {scene.name}. UI updates will not be available.");
            // Clear the reference if no UIManager is found in this scene
            uiManager = null;
        }
    }


    void Start()
    {
        // In Start, we only need to initialize BLE and start scanning if not already connected.
        // The UIManager reference will be set by OnSceneLoaded when the initial scene loads.

        if (!isConnected) // Only initialize and scan if not already connected from a previous session
        {
            // Initialize BLE interface
            BluetoothLEHardwareInterface.Initialize(true, false, () =>
            {
                // Initialization successful callback
                isConnected = false; // Set connected status to false initially
                // The initial UI update will be handled by OnSceneLoaded when the first scene loads.
                Debug.Log("BLE Initialized.");

                // Start scanning for peripherals
                ScanForDevice();

            },
            // Error callback during initialization
            (error) =>
            {
                Debug.LogError("BLE Initialization Error: " + error);
                isConnected = false; // Set connected status to false on error
                if (uiManager != null) // Try to update UI if a UIManager was found by OnSceneLoaded
                {
                    uiManager.UpdateConnectionStatus(isConnected); // Update UI
                    uiManager.UpdateMessage("BLE Init Error: " + error); // Show error message
                }
            });
        }
        else
        {
             Debug.Log("BLEManager already connected. Skipping initialization in Start.");
             // If already connected, ensure UI is updated for the current scene
             if (uiManager != null)
             {
                 uiManager.UpdateConnectionStatus(isConnected);
                 uiManager.UpdateMessage($"Already Connected. Pot: {latestPotValue:F2}, FSR: {latestFsrValue:F2}, ToF: {latestTofValue:F2}");
             }
        }
    }

    // Update is not typically needed for continuous data reception with SubscribeCharacteristic

    void ScanForDevice()
    {
         Debug.Log("Scanning for device: " + deviceName);
         if (uiManager != null)
         {
             uiManager.UpdateMessage("Scanning..."); // Update UI message
         }

        // Scan for peripherals using the 4-argument overload from the original script
        // The second callback takes only address and name (Action<string, string>).
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
            null, // Service UUIDs (string[]) - 1st arg
            // Found peripheral callback (Signature from original commented-out script) - 2nd arg
            (address, name) => // Action<string, string>
            {
                Debug.Log($"Found device: {name} at address: {address}");
                if (name.Contains(deviceName)) // Check if the device name matches
                {
                    Debug.Log($"Found target device: {deviceName}. Stopping scan.");
                    BluetoothLEHardwareInterface.StopScan(); // Stop scanning once found
                    ConnectToDevice(address); // Attempt to connect
                }
            },
            // Scan stopped callback (optional) - 3rd arg
            null, // Action<string> or null
            // Allow duplicates (bool) - 4th arg
            false); // bool
    }


    void ConnectToDevice(string address)
    {
        Debug.Log("Attempting to connect to " + address);
         if (uiManager != null)
         {
             uiManager.UpdateMessage("Connecting..."); // Update UI message
         }

        // Connect to the peripheral using the most likely 5-argument overload we identified.
        // The 4th callback confirms connection and might provide initial discovery details (Action<string, string, string>).
        // The 5th callback is for errors (Action<string>), potentially including disconnection.
        BluetoothLEHardwareInterface.ConnectToPeripheral(
            address,
            null, // Service Found Callback (unused in this flow) - Action<string, string> or null
            null, // Characteristic Found Callback (unused in this flow) - Action<string, string, string> or null
            // Connection Success / Initial Discovery Callback (Signature from our previous attempt based on StartingExample) - 4th arg
            (addressCallback, serviceUUIDCallback, characteristicUUIDCallback) => // Action<string, string, string>
            {
                // This callback should fire upon successful connection.
                // It might also be called for each service/characteristic discovered.
                // We set connected status and subscribe the first time it confirms connection.
                if (!isConnected) // Only process connection logic the first time this callback is hit for a new connection
                {
                     Debug.Log($"Connected to {addressCallback}. Initial Discovery - Service: {serviceUUIDCallback}, Char: {characteristicUUIDCallback}");
                    isConnected = true; // Set connected status to true
                    connectedDeviceAddress = addressCallback; // Store the connected address

                    if (uiManager != null)
                    {
                        uiManager.UpdateConnectionStatus(isConnected); // Update UI
                        uiManager.UpdateMessage("Connected. Subscribing..."); // Update UI message
                    }

                    // Proceed to subscribing after connection is confirmed.
                    // Data reception will happen via the SubscribeCharacteristicWithDeviceAddress callback.
                    BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                        connectedDeviceAddress, // Use the stored connected address
                        this.serviceUUID, // Use the class member serviceUUID
                        this.characteristicUUID, // Use the class member characteristicUUID,
                        // Subscribe success callback (optional)
                        null, // Action<string, string> or null
                        // Data received callback - This callback is where the sensor data (bytes) arrives
                        (addressData, characteristicUUIDData, bytes) => // Signature observed in StartingExample.cs Subscribe
                        {
                            // Data received from the characteristic
                            ProcessReceivedData(bytes); // Process the raw bytes
                        });
                }
                // If you needed to handle discovery of other specific services/characteristics,
                // you would add logic here using serviceUUIDCallback and characteristicUUIDCallback.
            },
            // Error callback (Signature from our previous attempt based on original code/errors) - 5th arg
            (error) => // Action<string>
            {
                Debug.LogError("BLE Connection or Error: " + error);
                // This callback might also signal disconnection or other errors.
                // We'll assume it means disconnected or an error occurred.
                isConnected = false; // Set connected status to false on error/disconnect
                connectedDeviceAddress = null; // Clear connected address

                if (uiManager != null) // Try to update UI if a UIManager was found by OnSceneLoaded
                {
                    uiManager.UpdateConnectionStatus(isConnected); // Update UI (assuming error means disconnected)
                    uiManager.UpdateMessage("BLE Error: " + error); // Show error message
                }
                // Optionally start scanning again after error
                ScanForDevice(); // Might need a delay or different handling for retries
            });

        // Note: With this 5-argument overload, there might not be a separate 'disconnected' callback parameter in ConnectToPeripheral.
    }

    // Process the raw byte data received from the SubscribeCharacteristicWithDeviceAddress callback
    void ProcessReceivedData(byte[] bytes)
    {
         // Assuming the data is an array of 3 floats (Pot, FSR, ToF) sent as bytes
         // Each float is 4 bytes, total 12 bytes.
         // You mentioned Pot and FSR are 0-4095 and ToF is 0-70mm.
         // Assuming they are sent as standard float (32-bit) converted to bytes.

        if (bytes != null && bytes.Length >= 12) // 3 values * 4 bytes/value = 12 bytes minimum
        {
            // Ensure the order matches what your ESP32 is sending
            // Assuming order is Pot (0-4095), FSR (0-4095), TOF (0-70mm) sent as floats
            try
             {
                // Corrected order to match ESP32: FSR, Pot, ToF
                latestFsrValue = BitConverter.ToSingle(bytes, 0); // First 4 bytes for FSR
                latestPotValue = BitConverter.ToSingle(bytes, 4); // Next 4 bytes for Pot
                latestTofValue = BitConverter.ToSingle(bytes, 8); // Last 4 bytes for ToF


                // Update UI message with the latest values (if UIManager is set up for this)
                if (uiManager != null && uiManager.messageText != null)
                {
                    uiManager.UpdateMessage($"Pot: {latestPotValue:F2}, FSR: {latestFsrValue:F2}, ToF: {latestTofValue:F2}");
                }
                // Also update debug text if available
                if (uiManager != null && uiManager.debugText != null)
                {
                    // You can put specific debug info here, or just echo the message
                    uiManager.UpdateDebug($"dinimueter");
                    // uiManager.UpdateDebug($"Raw Data: {BitConverter.ToString(bytes)}");
                }


                // You can also log the values to the console for debugging
                // Debug.Log($"Received Data - Pot: {latestPotValue}, FSR: {latestFsrValue}, ToF: {latestTofValue}");
            }
             catch (Exception ex)
             {
                 Debug.LogError($"Error processing received BLE data: {ex.Message}");
                  if (uiManager != null && uiManager.debugText != null)
                  {
                     uiManager.UpdateDebug($"Data processing error: {ex.Message}");
                  }
             }
        }
        else
        {
            // This warning might occur if the subscription sends empty or unexpected data initially
            Debug.LogWarning($"Received data bytes length ({bytes?.Length ?? 0}) is not as expected for 3 values (expected >= 12 bytes), or bytes is null.");
             if (uiManager != null && uiManager.debugText != null)
             {
                  uiManager.UpdateDebug($"Received unexpected data format. Length: {bytes?.Length ?? 0}");
             }
        }
    }

    void OnApplicationQuit()
    {
        // Clean up BLE resources when the application quits
         if (isConnected && !string.IsNullOrEmpty(connectedDeviceAddress))
        {
            // Attempt to disconnect the peripheral if the API supports it
            // If DisconnectPeripheral(address, callback) is not available in your API,
            // you might need to use DisconnectAll() or rely on DeInitialize.
             try
             {
                 // This might also trigger a disconnected event/callback if one exists and is separate.
                 BluetoothLEHardwareInterface.DisconnectPeripheral(connectedDeviceAddress, null);
             }
             catch (Exception ex)
             {
                 Debug.LogWarning($"Error attempting to disconnect peripheral: {ex.Message}. Relying on DeInitialize.");
             }
        }
        // Give a moment for disconnect before deinitialization (if DisconnectPeripheral was successful)
        // This is tricky in OnApplicationQuit, DeInitialize might be sufficient cleanup.

        BluetoothLEHardwareInterface.DeInitialize(() => {
            Debug.Log("BLE DeInitialized");
        });
    }

    // --- Method to explicitly disconnect if needed ---
    public void Disconnect()
    {
         if (isConnected && !string.IsNullOrEmpty(connectedDeviceAddress))
        {
             try
             {
                BluetoothLEHardwareInterface.DisconnectPeripheral(connectedDeviceAddress, null);
             }
             catch (Exception ex)
             {
                Debug.LogWarning($"Error attempting explicit disconnect: {ex.Message}.");
             }
        }
    }
    // -------------------------------------------------
}


// **Summary of Changes:**

// 1.  **`using UnityEngine.SceneManagement;`:** Added at the top to use `SceneManager`.
// 2.  **`OnEnable()` and `OnDisable()`:** Implemented to subscribe and unsubscribe from `SceneManager.sceneLoaded`. This is the standard pattern for managing event subscriptions in Unity.
// 3.  **`OnSceneLoaded(Scene scene, LoadSceneMode mode)`:** This new method is called by Unity whenever a scene is loaded.
//     * Inside this method, `uiManager = FindObjectOfType<UIManager>();` searches the newly loaded scene for an active GameObject with a `UIManager` component and assigns it to the `uiManager` variable.
//     * Added debug logs to track when a scene is loaded and whether a `UIManager` was found.
//     * Added logic to update the UI with the current connection status and latest data if a `UIManager` is found in the new scene.
// 4.  **`Start()` Method Adjustment:** The `Start()` method now primarily focuses on initializing BLE and starting the scan *only if* the device is not already connected. The initial UI update logic that was previously in `Start()` is now handled by the `OnSceneLoaded` method when the first scene loads.
// 5.  **UI Update Checks:** Added null checks for `uiManager`, `uiManager.messageText`, and `uiManager.debugText` before attempting to update them, making the script more robust if a scene doesn't have a `UIManager` or if the UI fields aren't assigned.

// **What you need to do in Unity:**

// 1.  **Replace the content of your `BLEManager.cs` file** with the code from the updated Canvas above.
// 2.  **Ensure `UIManager` GameObjects Exist in Each Scene:** Make sure that in your Calibration scene and your Main Game scene, you have a GameObject with the `UIManager.cs` script attached, and that its TextMeshProUGUI fields (`connectionStatus`, `messageText`, `debugText`) are correctly assigned in the Inspector.
// 3.  **Only One `BLEManager` GameObject:** Confirm that you *only* have the `BLEManager` GameObject in your initial scene (e.g., the Main Menu scene). The `DontDestroyOnLoad` in the `Awake()` method will ensure it persists.

// With these changes, your persistent `BLEManager` should now be able to find and update the `UIManager` in whichever scene is currently loaded, allowing the connection status and sensor data to be displayed correctly across your different scree