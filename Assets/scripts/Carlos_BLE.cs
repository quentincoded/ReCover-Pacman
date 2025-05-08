/* This script serves as the BLE connection manager for ESP32-Unity for ReCoverRun therapy.
It works with the esp32 sketch included at the bottom of this source file.
It is an adaptation of the StartingExample.cs file from Shatalmic.
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BLECommunication : MonoBehaviour

{
    public static BLECommunication Instance { get; private set; } // Singleton Instance

    public string DeviceName = "ReCover";
    public string ServiceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
    public string DATAUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";

    public float? fsrMean;
    public float? potMean;

    public enum States
    {
        None,
        Scan,
        ScanRSSI,
        ReadRSSI,
        Connect,
        RequestMTU,
        Subscribe,
        Unsubscribe,
        Disconnect,
    }

    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private string _deviceAddress;
    private bool _foundDATAUUID = false;
    private bool _rssiOnly = false;
    private int _rssi = 0;

    // public Text StatusText;
    // public Text ButtonPositionText;

    public States bleState {
        get { return _state; } // gives _state (from States enum)
        set { SetState(value, 2f); } // sets _state (through SetState)
    }

    private string StatusMessage
    {
        set
        {
            BluetoothLEHardwareInterface.Log(value);
            // StatusText.text = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Ensures the object persists
    }

    void Reset()
    {
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _deviceAddress = null;
        _foundDATAUUID = false;
        _rssi = 0;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    void StartProcess()
    {
        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            fsrMean = null;
            potMean = null;

            SetState(States.Scan, 2f);

        }, (error) =>
        {

            // StatusMessage = "Error during initialize: " + error;
        });
    }

    // Use this for initialization
    void Start()
    {
        StartProcess();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        // StatusMessage = "Scanning for " + DeviceName;
                        Debug.Log("Starting BLE Scan...");
    
                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
                        {
                            Debug.Log($"Found Device: {name} at {address}");

                            // if your device does not advertise the rssi and manufacturer specific data
                            // then you must use this callback because the next callback only gets called
                            // if you have manufacturer specific data

                            if (!_rssiOnly)
                            {
                                if (name.Contains(DeviceName))
                                {
                                    // StatusMessage = "Found " + name;

                                    // found a device with the name we want
                                    // this example does not deal with finding more than one
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                }
                            }

                        }, (address, name, rssi, bytes) =>
                        {

                            // use this one if the device responses with manufacturer specific data and the rssi

                            if (name.Contains(DeviceName))
                            {
                                // StatusMessage = "Found " + name;

                                if (_rssiOnly)
                                {
                                    _rssi = rssi;
                                }
                                else
                                {
                                    // found a device with the name we want
                                    // this example does not deal with finding more than one
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                }
                            }

                        }, _rssiOnly); // this last setting allows RFduino to send RSSI without having manufacturer data

                        if (_rssiOnly)
                            SetState(States.ScanRSSI, 0.5f);

                        break;

                    case States.ScanRSSI:
                        break;

                    case States.ReadRSSI:
                        // StatusMessage = $"Call Read RSSI";
                        BluetoothLEHardwareInterface.ReadRSSI(_deviceAddress, (address, rssi) =>
                        {
                            // StatusMessage = $"Read RSSI: {rssi}";
                        });

                        SetState(States.ReadRSSI, 2f);
                        break;

                    case States.Connect:
                        // StatusMessage = "Connecting...";

                        // set these flags
                        // _foundPOTUUID = false;
                        _foundDATAUUID = false;

                        // note that the first parameter is the address, not the name. I have not fixed this because
                        // of backwards compatiblity.
                        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) =>
                        {
                            // StatusMessage = "Connected...";

                            BluetoothLEHardwareInterface.StopScan();

                            if (IsEqual(serviceUUID, ServiceUUID))
                            {

                                _foundDATAUUID = _foundDATAUUID || IsEqual(characteristicUUID, DATAUUID);

                                // if we have found both characteristics that we are waiting for
                                // set the state. make sure there is enough timeout that if the
                                // device is still enumerating other characteristics it finishes
                                // before we try to subscribe
                                if (_foundDATAUUID)
                                {
                                    _connected = true;
                                    SetState(States.RequestMTU, 2f);
                                }
                            }
                        }, (disconnectDeviceAddress) => // Callback for disconnection
                        {
                            // if this is called, the device identitied with the passed in parameter
                            // just disconnected.
                            // You can start scanning again by setting the state to Scan like so:
                            _connected = false;
                            SetState(States.Scan, 0.1f);
                        });
                        break;

                    case States.RequestMTU:
                        // StatusMessage = "Requesting MTU";

                        BluetoothLEHardwareInterface.RequestMtu(_deviceAddress, 185, (address, newMTU) =>
                        {
                            // StatusMessage = "MTU set to " + newMTU.ToString();

                            SetState(States.Subscribe, 0.1f);
                        });
                        break;

                    case States.Subscribe:
                        // StatusMessage = "Subscribing to characteristics...";
                        
                        // Data characteristic -> 4 bytes
                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                            _deviceAddress,
                            ServiceUUID,
                            DATAUUID,
                            null,
                            (address, characteristicUUID, bytes) =>
                            {

                                // Data package in Little Endian format
                                // FSR
                                int fsrInt = bytes[0] | (bytes[1] << 8);
                                fsrMean = fsrInt/10.0f;
                                // POT
                                int potInt = bytes[2] | (bytes[3] << 8);
                                potMean = potInt/10.0f;


                            });

                        break;

                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID, DATAUUID, null);
                        SetState(States.Disconnect, 4f);
                        fsrMean = null;
                        potMean = null;
                        break;

                    case States.Disconnect:
                        // StatusMessage = "Commanded disconnect.";

                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) =>
                            {
                                // StatusMessage = "Device disconnected";
                                BluetoothLEHardwareInterface.DeInitialize(() =>
                                {
                                    _connected = false;
                                    _state = States.None;
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() =>
                            {
                                _state = States.None;
                            });
                        }
                        break;
                }
            }
        }
    }

    string FullUUID(string uuid)
    {
        string fullUUID = uuid;
        if (fullUUID.Length == 4)
            fullUUID = "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

        return fullUUID;
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }

}

