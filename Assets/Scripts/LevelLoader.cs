using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    
    public Animator transition;
    public float transitionTime = 0.5f;
    public MidiNotes midiNotes;

    public void StartTransitionCoroutine(int sceneIndex)
    {
        StartCoroutine(TransitionToScene(sceneIndex));
    } 

    IEnumerator TransitionToScene(int sceneIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        UIButtons.Instance.LoadScene(sceneIndex);
        //yield return new WaitUntil(() => !midiNotes.isQueueEmpty());
    }
    
    /*public void StartRestartLevelCoroutine()
    {
        StartCoroutine(RestartLevelRoutine());
    } 
    
    IEnumerator RestartLevelRoutine()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        UIButtons.Instance.RestartLevel();
    }*/

}
