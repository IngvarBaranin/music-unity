using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PythonCommunication : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI sendToPythonText = null;

    UdpSocket udpSocket;

    public void SendToPython(string messageToPython)
    {
        udpSocket.SendData(messageToPython);
    }

    private void Start()
    {
        udpSocket = FindObjectOfType<UdpSocket>();
        sendToPythonText.text = "Ready for prediction";
    }
    
    public void QuitApp()
    {
        print("Quitting");
        Application.Quit();
    }
}
