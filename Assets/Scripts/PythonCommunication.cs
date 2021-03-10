using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PythonCommunication : MonoBehaviour
{
    UdpSocket udpSocket;

    public void SendToPython(string messageToPython)
    {
        udpSocket.SendData(messageToPython);
    }

    private void Start()
    {
        udpSocket = FindObjectOfType<UdpSocket>();
    }
}
