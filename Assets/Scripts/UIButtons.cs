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

    public TMP_Dropdown TMPDropdown;
    string[] playerPrefNames = {"Reactive", "Static", "NoMusic"};
    
    public bool isInGame = true;

    private void Start()
    {
        Instance = this;

        if (!isInGame)
        {
            for (int i = 0; i < playerPrefNames.Length; i++)
            {
                PlayerPrefs.SetInt(playerPrefNames[i], 0);
            }

            PlayerPrefs.SetInt("Reactive", 1);
            
            TMPDropdown.onValueChanged.AddListener(delegate {
                HandlePlayerPref(TMPDropdown);
            });
        }
        
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

    public void HandlePlayerPref(TMP_Dropdown tmpDropdown)
    {
        int valIdx = tmpDropdown.value;

        for (int i = 0; i < 3; i++)
        {
            if (valIdx == i) PlayerPrefs.SetInt(playerPrefNames[i], 1);
            else PlayerPrefs.SetInt(playerPrefNames[i], 0);
        }

        for (int i = 0; i < 3; i++)
        {
            Debug.Log(PlayerPrefs.GetInt(playerPrefNames[i]));
        }
        
    }
    
}
