using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{

    public GameObject player;
    public float DeathZoneYPos = -5;
    public Transform RespawnLocation;

    private void Update()
    {
        transform.position = new Vector3(player.transform.position.x, DeathZoneYPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            other.gameObject.transform.position = RespawnLocation.position;
        }
    }
    
}
