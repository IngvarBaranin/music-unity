using System;
using System.Collections;
using System.Collections.Generic;
using MidiPlayerTK;
using UnityEngine;

public class MidiNotes : MonoBehaviour
{
    
    public MidiStreamPlayer midiStreamPlayer;
    public PythonCommunication pythonCommunication;
    
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
    }

    public void addToQueue(string notesString)
    {
        notesQueue.Enqueue(notesString);
    }

    public string getDequeue()
    {
        return notesQueue.Dequeue();
    }

    public void playIncomingNotes(string notes)
    {

        List<MPTKEvent> listOfEvents = new List<MPTKEvent>();

        string[] noteArray = notes.Split(' ');
        Debug.Log(noteArray.Length);

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

    void Update()
    {
        milliSecondsSinceFirstNote = MilliSecondTimer(firstNoteStartInMilliseconds);
        milliSecondsDifferenceBetweenEndAndStart = midiStreamLastNoteEnd - firstNoteStartInMilliseconds;

        //Debug.Log("firstNoteStartInMilliseconds " + firstNoteStartInMilliseconds);
        //Debug.Log("milliSecondsSinceFirstNote " + milliSecondsSinceFirstNote);
        //Debug.Log("midiStreamLastNoteEnd " + midiStreamLastNoteEnd);
        //Debug.Log("milliSecondsDifferenceBetweenEndAndStart " + milliSecondsDifferenceBetweenEndAndStart);

        if (Input.GetMouseButton(0))
        {
            if (milliSecondsSinceFirstNote > milliSecondsDifferenceBetweenEndAndStart)
            {
                if (notesQueue.Count != 0)
                {
                    playIncomingNotes(notesQueue.Dequeue());
                }
                Debug.Log("Telling Python to predict");
            }
        }

        pythonCommunication.SendToPython("True");
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Changing instrument to index " + currentInstrumentIndex + 1);
            currentInstrumentIndex += 1;
            midiStreamPlayer.MPTK_PlayEvent(
                new MPTKEvent()
                {
                    Command = MPTKCommand.PatchChange, 
                    Value = currentInstrumentIndex, 
                    Channel = 0
                });
        }
    }
    
}
