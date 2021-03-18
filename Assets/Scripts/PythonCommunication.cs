using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PythonCommunication : MonoBehaviour
{
    UdpSocket udpSocket;
    public Transform playerPosition;
    public MidiNotes midiNotes;

    private string predictionHeight = "1";
    
    private string PredictHigherOrLower(float posY)
    {
        string previousHeight = predictionHeight;
        
        if (posY > 8 && midiNotes.isMusicReactive)
        {
            predictionHeight = "2";
        } else if (posY < 4 && midiNotes.isMusicReactive)
        {
            predictionHeight = "0";
        }
        else
        {
            predictionHeight = "1";
        }

        if (previousHeight != predictionHeight)
        {
            midiNotes.clearQueue();
        }

        return predictionHeight;
    }

    public void SendToPython(string messageToPython)
    {
        if (messageToPython == "Quit")
        {
            udpSocket.SendData(messageToPython + ":0");
            return;
        }
        
        udpSocket.SendData(messageToPython + ":" + PredictHigherOrLower(playerPosition.transform.position.y));
    }

    private void Start()
    {
        udpSocket = FindObjectOfType<UdpSocket>();
    }
}
