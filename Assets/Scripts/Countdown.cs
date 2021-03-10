using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{

    public TextMeshProUGUI countdownText;
    public int countdownSeconds = 3;
    public bool gameStarted = false;

    public static Countdown Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(CountdownTimer());
    }

    IEnumerator CountdownTimer()
    {
        while (countdownSeconds > 0)
        {
            countdownText.text = countdownSeconds.ToString();
            yield return new WaitForSeconds(1f);
            countdownSeconds--;
        }

        gameStarted = true;
        countdownText.gameObject.SetActive(false);
    }
    
}
