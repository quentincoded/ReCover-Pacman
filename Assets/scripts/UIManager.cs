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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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
