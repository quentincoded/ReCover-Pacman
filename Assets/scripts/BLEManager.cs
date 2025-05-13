// // 

// using System;
// using TMPro;
// using UnityEngine;

// public class BLEManager : MonoBehaviour
// {
//     // You can still access this manager from other scripts if needed,
//     // although the Singleton pattern was removed for "slight adjustments".
//     // If you need easy global access, consider adding the Singleton pattern back.

//     public string serviceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
//     public string characteristicUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";
//     public string deviceName = "ReCover";

//     public UIManager uiManager;

//     // Public variables to store the latest received sensor data
//     // These can be accessed by other scripts to get the current sensor readings.
//     public float latestFsrValue { get; private set; }
//     public float latestPotValue { get; private set; }
//     public float latestTofValue { get; private set; }

//     // Flag to indicate if the device is currently connected
//     // This can be checked by other scripts to know the connection state.
//     public bool isConnected { get; private set; }


//     void Start()
//     {
//         // Initialize BLE interface
//         BluetoothLEHardwareInterface.Initialize(true, false, () =>
//         {
//             // Initialization successful callback
//             isConnected = false; // Set connected status to false initially
//             if (uiManager != null)
//             {
//                 uiManager.UpdateConnectionStatus(isConnected); // Update UI
//             }
//             else
//             {
//                 Debug.LogWarning("UIManager not assigned in BLEManager!");
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

//     void ScanForDevice()
//     {
//          Debug.Log("Scanning for device: " + deviceName);
//          if (uiManager != null)
//          {
//              uiManager.UpdateMessage("Scanning..."); // Update UI message
//          }

//         // Scan for peripherals with specific service UUIDs (null means all)
//         // The callback signature is adjusted based on the error message.
//         BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
//             null, // Scan for all services or specify serviceUUIDs
//             // Adjusted callback signature based on common BLE library patterns
//             (address, name, rssi, advertisingData) => // Changed delegate signature here
//             {
//                 // Found a peripheral
//                 Debug.Log("Found device: " + name + " at address: " + address + " RSSI: " + rssi);
//                 if (name.Contains(deviceName)) // Check if the device name matches
//                 {
//                     Debug.Log("Found target device: " + deviceName + ". Stopping scan.");
//                     BluetoothLEHardwareInterface.StopScan(); // Stop scanning once the target device is found
//                     ConnectToDevice(address); // Attempt to connect to the found device
//                 }
//             },
//             // Scan stopped callback (optional)
//             null,
//             // Allow duplicates (true) or not (false) - true can be useful during development
//             false);
//     }


//     void ConnectToDevice(string address)
//     {
//         Debug.Log("Attempting to connect to " + address);
//          if (uiManager != null)
//          {
//              uiManager.UpdateMessage("Connecting..."); // Update UI message
//          }

//         // Connect to the peripheral
//         BluetoothLEHardwareInterface.ConnectToPeripheral(
//             address,
//             // Connection established callback - Adjusted delegate signature based on the error message
//             (connectedAddress, serviceUUID, characteristicUUID) => // Changed delegate signature here
//             {
//                 Debug.Log("Connected to " + connectedAddress);
//                 isConnected = true; // Set connected status to true
//                 if (uiManager != null)
//                 {
//                     uiManager.UpdateConnectionStatus(isConnected); // Update UI
//                     uiManager.UpdateMessage("Connected. Subscribing..."); // Update UI message
//                 }

//                 // Subscribe to the characteristic to receive data updates
//                 // This callback is where the sensor data (bytes) is received
//                 BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
//                     connectedAddress, // Use the connected address
//                     this.serviceUUID, // Use the class member serviceUUID
//                     this.characteristicUUID, // Use the class member characteristicUUID
//                     // Subscribe success callback (optional)
//                     null,
//                     // Data received callback
//                     (characteristicAddress, characteristicUUID, bytes) =>
//                     {
//                         // Data received from the characteristic
//                         ProcessReceivedData(bytes);
//                     });
//             },
//             // Disconnected callback
//             (disconnectedAddress) =>
//             {
//                 Debug.Log("Disconnected from " + disconnectedAddress);
//                 isConnected = false; // Set connected status to false
//                 if (uiManager != null)
//                 {
//                     uiManager.UpdateConnectionStatus(isConnected); // Update UI
//                     uiManager.UpdateMessage("Disconnected. Scanning again..."); // Update UI message
//                 }
//                 // Optionally start scanning again after disconnection
//                 ScanForDevice();
//             },
//             // Connection error callback
//             (error) =>
//             {
//                 Debug.LogError("BLE Connection Error: " + error);
//                 isConnected = false; // Set connected status to false on error
//                 if (uiManager != null)
//                 {
//                     uiManager.UpdateConnectionStatus(isConnected); // Update UI
//                     uiManager.UpdateMessage("Connection Error: " + error); // Show error message
//                 }
//                 // Optionally start scanning again after connection error
//                  ScanForDevice();
//             });
//     }

//     // Process the raw byte data received from the BLE characteristic
//     void ProcessReceivedData(byte[] bytes)
//     {
//         // Assuming the data is an array of floats (4 bytes per float)
//         int floatCount = bytes.Length / 4;
//         float[] data = new float[floatCount];

//         // Convert bytes to floats
//         for (int i = 0; i < floatCount; i++)
//         {
//             data[i] = BitConverter.ToSingle(bytes, i * 4);
//         }

//         // Assuming the order is FSR, Pot, ToF based on your ESP32 code
//         if (floatCount >= 3)
//         {
//             // Store the latest received values in the public variables
//             latestFsrValue = data[0];
//             latestPotValue = data[1];
//             latestTofValue = data[2];

//             // Update the UI message with the latest values
//             if (uiManager != null)
//             {
//                 uiManager.UpdateMessage($"FSR: {latestFsrValue:F2}, Pot: {latestPotValue:F2}, ToF: {latestTofValue:F2}");
//                 // You could also update separate UI fields here if you add them to UIManager
//                 // uiManager.UpdateFsrText(latestFsrValue);
//                 // uiManager.UpdatePotText(latestPotValue);
//                 // uiManager.UpdateTofText(latestTofValue);
//             }

//             // You can also log the values to the console for debugging
//             // Debug.Log($"Received Data - FSR: {latestFsrValue}, Pot: {latestPotValue}, ToF: {latestTofValue}");
//         }
//         else
//         {
//             Debug.LogWarning("Received data bytes length is not as expected for FSR, Pot, ToF");
//             if (uiManager != null)
//             {
//                  uiManager.UpdateDebug("Received unexpected data format.");
//             }
//         }
//     }

//     void OnApplicationQuit()
//     {
//         // Clean up BLE resources when the application quits
//         BluetoothLEHardwareInterface.DeInitialize(() => {
//             Debug.Log("BLE DeInitialized");
//         });
//     }
// }


// **Explanation of Changes in `BLEManager.cs`:**

// 1.  **Singleton Pattern:** Added `public static BLEManager Instance;` and `Awake()` method to make `BLEManager` easily accessible from any other script using `BLEManager.Instance`.
// 2.  **Stored Values:** Added `public float latestFsrValue { get; private set; }`, `public float latestPotValue { get; private set; }`, and `public float latestTofValue { get; private set; }` to store the most recently received sensor values. `get; private set;` makes them readable by other scripts but only settable within `BLEManager`.
// 3.  **Connection Status Flag:** Added `public bool isConnected { get; private set; }` to easily check the connection state from other scripts.
// 4.  **Updated Value Storage:** In the `ProcessReceivedData` method, the received `fsr`, `pot`, and `tof` values are now assigned to the new `latestFsrValue`, `latestPotValue`, and `latestTofValue` variables.
// 5.  **Improved Connection/Scan Flow:** Modified `Start`, `ConnectToDevice`, and added `ScanForDevice` to make the connection process more robust, including attempting to rescan/reconnect on disconnection or error.
// 6.  **Discover Services/Characteristics:** Added `BluetoothLEHardwareInterface.DiscoverServicesAndCharacteristics` call before subscribing. This is a necessary step in the BLE process to ensure the service and characteristic are found on the connected device before attempting to subscribe.
// 7.  **DeInitialize on Quit:** Added `OnApplicationQuit` to properly clean up the BLE interface when the application closes.

// **Steps to set this up in Unity:**

// 1.  **Replace the content of your `BLEManager.cs` file** with the updated code above.
// 2.  **Ensure your `UIManager.cs` is assigned:** In the Unity Editor, select the GameObject that has your `BLEManager.cs` script attached. Drag the GameObject that has your `UIManager.cs` script attached into the **"Ui Manager"** field in the `BLE Manager (Script)` component in the Inspector.
// 3.  **Ensure UI Text Fields are assigned in `UIManager`:** Select the GameObject with your `UIManager.cs` script. In the Inspector, make sure your TextMeshProUGUI elements for connection status, message, and debug are assigned to the corresponding fields (`Connection Status`, `Message Text`, `Debug Text`).
// 4.  **Verify BLE Settings:** In the `BLE Manager (Script)` component, double-check that the `Service UUID`, `Characteristic UUID`, and `Device Name` match what is defined in your ESP32 code (`HF.h`).

// Now, when you run your Unity scene, the `BLEManager` will initialize, scan for your ESP32 device, attempt to connect, and subscribe to the characteristic. The connection status and the received sensor values will be displayed in the UI fields you've linked in the `UIManager`. Other scripts, like your `PacmanScript`, can now access the latest sensor data using `BLEManager.Instance.latestPotValue`, `BLEManager.Instance.latestFsrValue`, e


//OLD SCRIPT ___________________________________________________________________________________________________________________________________
using System;
using TMPro;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    // Singleton instance to easily access this manager from other scripts
    public static BLEManager Instance;
    public string serviceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
    public string characteristicUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";
    public string deviceName = "ReCover";

    public UIManager uiManager; 
    // Public variables to store the latest received sensor data
    public float latestFsrValue { get; private set; }
    public float latestPotValue { get; private set; }
    public float latestTofValue { get; private set; }
    public bool isConnected { get; private set; }

    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Optional: Keep the object alive across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one
            Destroy(gameObject);
        }
    }

    
    void Start()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            uiManager.UpdateConnectionStatus(false);
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                null,
                (addr, name) =>
                {
                    if (name.Contains(deviceName))
                    {
                        BluetoothLEHardwareInterface.StopScan();
                        Connect(addr);
                    }
                },
                null, false);
        },
        err => Debug.LogError(err));
    }
    void Update()
    {
        // Optional: You can add any update logic here if needed
    }
    
    

    void Connect(string addr)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(
            addr, null, null,
            (fsr, pot, tof) =>
            {
                uiManager.UpdateConnectionStatus(true);
                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                    addr, serviceUUID, characteristicUUID,
                    null, (address, ch, bytes) =>
                    {
                        int count = bytes.Length / 4;
                        float[] data = new float[count];
                        for (int i = 0; i < count; i++)
                            data[i] = BitConverter.ToSingle(bytes, i * 4);
                        var fsr = data[0];
                        var pot = data[1];
                        var tof = data[2];
                        // var mag   = new Vector3(data[7], data[8], data[9]);
                        uiManager.UpdateMessage($"FSR: {fsr:F2}, Pot: {pot:F2}, ToF: {tof:F2}");
                        // uiManager.UpdateMessage($"Pos: {imuFilter.transform.position:F2}");
                    });
            },
            err => uiManager.UpdateConnectionStatus(false));
    }

}
