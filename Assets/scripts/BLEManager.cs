using System;
using TMPro;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    public string serviceUUID = "9f3c872e-2f1b-4c58-bc2a-5a2e2f48f519";
    public string characteristicUUID = "2d8e1b65-9d11-43ea-b0f5-c51cb352ddfa";
    public string deviceName = "ReCover";

    public UIManager uiManager; 

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
