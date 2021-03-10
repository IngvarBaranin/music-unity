using System;
using System.Collections;
using System.Collections.Generic;
using MidiPlayerTK;
using UnityEngine;

public class MidiNotes : MonoBehaviour
{
    
    public MidiStreamPlayer midiStreamPlayer;
    public PythonCommunication pythonCommunication;
    public PlayerMovement playerMovement;
    
    private long firstNoteStartInMilliseconds = 0;
    private long midiStreamLastNoteEnd = 0;
    private long milliSecondsSinceFirstNote = 0;
    private long milliSecondsDifferenceBetweenEndAndStart = 0;

    private int currentInstrumentIndex = 0;

    private Queue<string> notesQueue;
    
    [Range(0.5f, 2f)]
    public float musicSpeed = 1;
    
    [Range(0f, 1f)]
    public float musicVelocity = 1;

    private void Start()
    {
        notesQueue = new Queue<string>();
        ChangeInstrument(4);
    }

    public void addToQueue(string notesString)
    {
        notesQueue.Enqueue(notesString);
    }

    public bool isQueueEmpty()
    {
        return notesQueue.Count == 0;
    }

    public void playIncomingNotes(string notes)
    {

        List<MPTKEvent> listOfEvents = new List<MPTKEvent>();

        string[] noteArray = notes.Split(' ');
        //Debug.Log(noteArray.Length);

        foreach (string noteString in noteArray)
        {
            string[] noteInfo = noteString.Split(':');
            
            int noteMidiNumber = Int32.Parse(noteInfo[0]);
            int noteOffset = (int) Math.Ceiling(1000 * float.Parse(noteInfo[1]) * (1 / musicSpeed));
            int noteDuration = (int) Math.Ceiling(1000 * float.Parse(noteInfo[2]) * (1 / musicSpeed));
            int noteVelocity = (int) Math.Floor(Int32.Parse(noteInfo[3]) * musicVelocity);
            
            /*Debug.Log("noteMidiNumber " +  noteMidiNumber);
            Debug.Log("noteOffset " +  noteOffset);
            Debug.Log("noteDuration " +  noteDuration);
            Debug.Log("noteVelocity " +  noteVelocity);*/
            
            MPTKEvent noteToPlay = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn, // midi command
                Value = noteMidiNumber, // from 0 to 127, 48 for C4, 60 for C5, ...
                Channel = 0, // from 0 to 15, 9 reserved for drum
                Duration = noteDuration, // note duration in millisecond, -1 to play undefinitely, MPTK_StopChord to stop
                Velocity = noteVelocity, // from 0 to 127, sound can vary depending on the velocity
                Delay = noteOffset, // delay in millisecond before playing the note
            };
            
            listOfEvents.Add(noteToPlay);
            midiStreamLastNoteEnd = (DateTime.Now.Ticks / 10000) + noteOffset + noteDuration; // Last note ends after right now + its offset and duration
        }

        firstNoteStartInMilliseconds = DateTime.Now.Ticks / 10000; // First note starts now 
        midiStreamPlayer.MPTK_PlayEvent(listOfEvents);
    }

    long MilliSecondTimer(long startMilliSeconds)
    {
        return (DateTime.Now.Ticks / 10000) - startMilliSeconds;  //(int) Math.Ceiling(Time.time * 1000 - startMilliSeconds);
    }
    
    private float Remap(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private int PerfectFifthTranspose(float posY)
    {
        if (posY > 8) return 7;
        if (posY < 3) return -7;
        return 0;
    }

    private float MusicSpeedTransform(float runSpeed)
    {
        if (runSpeed >= 1.45f) return 1.5f;
        return 0.75f;
    }

    private void Update()
    {
        float playerPosY = playerMovement.gameObject.transform.position.y;
        midiStreamPlayer.transpose = PerfectFifthTranspose(playerPosY);
        musicSpeed = MusicSpeedTransform(playerMovement.sprintMultiplier);
    }

    void FixedUpdate()
    {
        milliSecondsSinceFirstNote = MilliSecondTimer(firstNoteStartInMilliseconds);
        milliSecondsDifferenceBetweenEndAndStart = midiStreamLastNoteEnd - firstNoteStartInMilliseconds;

        //Debug.Log("firstNoteStartInMilliseconds " + firstNoteStartInMilliseconds);
        //Debug.Log("milliSecondsSinceFirstNote " + milliSecondsSinceFirstNote);
        //Debug.Log("midiStreamLastNoteEnd " + midiStreamLastNoteEnd);
        //Debug.Log("milliSecondsDifferenceBetweenEndAndStart " + milliSecondsDifferenceBetweenEndAndStart);

        if (playerMovement.horizontalMove != 0 || playerMovement.jump)
        {
            if (milliSecondsSinceFirstNote > milliSecondsDifferenceBetweenEndAndStart)
            {
                if (notesQueue.Count != 0)
                {
                    playIncomingNotes(notesQueue.Dequeue());
                }
            }
        }

        pythonCommunication.SendToPython("True"); // Telling Python to predict
    }

    public void ChangeInstrument(int instrumentIndex)
    {
        midiStreamPlayer.MPTK_PlayEvent(
            new MPTKEvent()
            {
                Command = MPTKCommand.PatchChange, 
                Value = instrumentIndex, 
                Channel = 0
            });
    }

    public void ChangeInstrumentForPlatform(string platfromName)
    {
        switch (platfromName)
        {
            case "BlueRule":
                ChangeInstrument(4);
                break;
            case "GreenRule":
                ChangeInstrument(10);
                break;
            case "LightBlueRule":
                ChangeInstrument(26);
                break;
            default:
                //ChangeInstrument(0);
                break;
        }
    }
    
}
