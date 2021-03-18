using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour
{
   
    public static UIButtons Instance;

    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI currentScoreText;
    public PlayerMovement playerMovement;
    public Toggle VolumeToggle;
    public Toggle ReactiveToggle;

    public bool isInGame = true;

    private void Start()
    {
        Instance = this;
        
        SetMusicVolumeToggle();
        SetReactiveMusicToggle();

        if (isInGame)
        {
            currentScoreText.text = "0";
            bestScoreText.text = "0";
        }
    }

    private void Update()
    {
        if (isInGame)
        {
            UpdateScore();
        }
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Debug.Log("quitting");
        Application.Quit();
    }
    
    public void UpdateScore()
    {
        int playerScore = (int) Math.Floor(playerMovement.transform.position.x);
        if (playerScore > 0)
        {
            currentScoreText.text = playerScore.ToString();
            if (playerScore > Int32.Parse(bestScoreText.text))
            {
                bestScoreText.text = playerScore.ToString();
            }
        }
    }

    public void SetMusicVolumeToggle()
    {
        Debug.Log("Setting Volume to " + (VolumeToggle.isOn ? 1 : 0));
        PlayerPrefs.SetInt("Volume", VolumeToggle.isOn ? 1 : 0);

        ReactiveToggle.interactable = VolumeToggle.isOn;
    }

    public void SetReactiveMusicToggle()
    {
        Debug.Log("Setting Reactive to " + (ReactiveToggle.isOn ? 1 : 0));
        PlayerPrefs.SetInt("Reactive", ReactiveToggle.isOn ? 1 : 0);
    }
    
}
