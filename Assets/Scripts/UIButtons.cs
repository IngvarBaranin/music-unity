using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{
   
    public static UIButtons Instance;

    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI currentScoreText;
    public PlayerMovement playerMovement;

    public bool isInGame = true;

    private void Start()
    {
        Instance = this;

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
    
}
