using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PythonTest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI pythonRcvdText = null;
    [SerializeField] TextMeshProUGUI sendToPythonText = null;

    string tempStr = "Waiting for predictions";
    UdpSocket udpSocket;

    public void UpdatePythonRcvdText(string str)
    {
        tempStr = str;
    }

    public void SendToPython()
    {
        udpSocket.SendData("True");
    }

    private void Start()
    {
        udpSocket = FindObjectOfType<UdpSocket>();
        sendToPythonText.text = "Ready for prediction";
    }

    void Update()
    {
        pythonRcvdText.text = tempStr;
    }
    
    public void QuitApp()
    {
        print("Quitting");
        Application.Quit();
    }
}
