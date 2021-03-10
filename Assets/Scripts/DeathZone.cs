using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{

    public static DeathZone Instance;
    
    public GameObject player;
    public float DeathZoneYPos = -5;
    public Transform RespawnLocation;

    public CameraMovement mainCameraMovement;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        transform.position = new Vector3(player.transform.position.x, DeathZoneYPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            Restart();
        }
    }

    public void Restart()
    {
        player.gameObject.transform.position = RespawnLocation.position;
        mainCameraMovement.ResetCameraSmooth();
    }
}
