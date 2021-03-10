/*
Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

Use at your own risk
Use under the Apache License 2.0

Modified by: 
Youssef Elashry 12/2020 (replaced obsolete functions and improved further - works with Python as well)
Based on older work by Sandra Fang 2016 - Unity3D to MATLAB UDP communication - [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MidiPlayerTK;
using Debug = UnityEngine.Debug;

public class UdpSocket : MonoBehaviour
{
    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "127.0.0.1"; // local host
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    PythonCommunication pythonCommunication;
    private Process pythonServer;

    public MidiNotes midiNotes;
    public bool useServer;

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    void Awake()
    {

        if (useServer)
        {
            pythonServer = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:/WINDOWS/system32/cmd.exe"; 
            startInfo.Arguments = @"/c cd " + Application.streamingAssetsPath + " && start /min python \"" + Application.streamingAssetsPath + "/server.py\""; 
            pythonServer.StartInfo = startInfo;
            pythonServer.Start();
            Thread.Sleep(7000); // Wait for server to be ready
        }

        //C:/WINDOWS/system32/cmd.exe /c start /min python "C:/Users/PC/Desktop/Unity projects/MusicGen/Assets/StreamingAssets/server.py"
        
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");
    }

    private void Start() 
    {
        pythonCommunication = FindObjectOfType<PythonCommunication>(); // Instead of using a public variable
    }

    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                List<string> noteChunks = notesToChunks(text, 5);
                foreach (string noteChunk in noteChunks)
                {
                    midiNotes.addToQueue(noteChunk);
                }
                //midiNotes.addToQueue(text);
                //midiNotes.playIncomingNotes(text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private List<string> notesToChunks(string text, int chunkSize)
    {
        
        List<string> chunks = new List<string>();

        // "data:1:2:3 data:1:2:3 data:1:2:3 ..." - > [data:1:2:3, data:1:2:3, ..., data:1:2:3]
        string[] noteArray = text.Split(' '); 
        
        for (int i = 0; i < (noteArray.Length / chunkSize); i++)
        {
            string[] chunkOfNoteData = noteArray.Skip(i*chunkSize).Take(chunkSize).ToArray(); // [note:data note:data ... note:data] (chunkSize)
            
            float firstNoteOffset = 0;
            string[] tempChunk = new string[chunkSize];
            for (int j = 0; j < chunkOfNoteData.Length; j++)
            {
                
                string[] noteData = chunkOfNoteData[j].Split(':');
                if (j == 0) firstNoteOffset = float.Parse(noteData[1]);

                string newNoteOffset = (float.Parse(noteData[1]) - firstNoteOffset).ToString();
                noteData[1] = newNoteOffset;
                
                tempChunk[j] = string.Join(":", noteData);
            }

            chunks.Add(string.Join(" ", tempChunk));
        }
        
        return chunks;
    }

    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE
        //pythonTest.UpdatePythonRcvdText(input); // Update text by string received from python

        if (!isTxStarted) // First data arrived so tx started
        {
            isTxStarted = true;
        }
    }
    
    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        pythonCommunication.SendToPython("Quit");
        
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
    
    

}