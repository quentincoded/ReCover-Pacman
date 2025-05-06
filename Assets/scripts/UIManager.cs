using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI connectionStatus;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI debugText; // assign in Inspector

    public void UpdateConnectionStatus(bool connected)
    {
        connectionStatus.text = connected ? "Connected" : "Disconnected";
    }

    public void UpdateMessage(string msg)
    {
        messageText.text = msg;
    }

    public void UpdateDebug(string debugMsg)
    {
        if (debugText != null)
            debugText.text = debugMsg;
    }
}
