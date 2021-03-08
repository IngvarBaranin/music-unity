﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;
    public float horizontalMove = 0f;
    public bool jump = false;

    public float sprintModifier = 1.5f;
    private float sprintMultiplier = 1f;
    
    public ParticleSystem sprintWind;

    private void Start()
    {
        if (!sprintWind.isPlaying) sprintWind.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 windOriginalPos = sprintWind.gameObject.transform.position;
        sprintWind.gameObject.transform.position = new Vector3(gameObject.transform.position.x + 10, windOriginalPos.y, 0);
        var sprintWindMain = sprintWind.main;
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprintMultiplier = sprintModifier;
            sprintWindMain.startSpeedMultiplier = 25f;
        }
        else
        {
            sprintMultiplier = 1f;
            sprintWindMain.startSpeedMultiplier = 20f;
        }
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed * sprintMultiplier;
        
        animator.SetFloat("Speed", Math.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }
}
