﻿using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.

	const float k_GroundedRadius = 0.165f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	private string lastTileBeneathFeet = "BlueRule";
	
	public Animator animator;
	public MidiNotes midiNotes;
	public ParticleSystem walkingParticles;
	private Vector2 walkingParticleThreshold = new Vector2(0.1f, 0.1f);
	
	public BoxCollider2D playerCollider;
	private Vector2 colliderOriginalSize;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
	}
	
	private void Start()
	{
		colliderOriginalSize = playerCollider.size;
	}
	
	void OnDrawGizmosSelected()
	{
		// Draw a yellow sphere at the transform's position
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(m_GroundCheck.position, k_GroundedRadius);
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				Tilemap tilemapObject = colliders[i].gameObject.GetComponent<Tilemap>();
				Vector3 groundCheckLower = new Vector3(m_GroundCheck.transform.position.x, m_GroundCheck.transform.position.y - 0.5f);
				TileBase tileBeneathFeet = tilemapObject.GetTile(tilemapObject.WorldToCell(groundCheckLower));
				
				m_Grounded = true;
				if (tileBeneathFeet != null)
				{
					if (tileBeneathFeet.name != lastTileBeneathFeet)
					{
						lastTileBeneathFeet = tileBeneathFeet.name;
						if (midiNotes.isMusicEnabled && midiNotes.isMusicReactive)
						{
							midiNotes.ChangeInstrumentForPlatform(tileBeneathFeet.name);
						}
					}
				}
				break;
			}
		}

		if (!m_Grounded)
		{
			playerCollider.size = new Vector2(colliderOriginalSize.x, colliderOriginalSize.y * 0.5f);
			animator.SetBool("isJumping", true);
		}
		else
		{
			playerCollider.size = colliderOriginalSize;
			animator.SetBool("isJumping", false);
		}
	}


	public void Move(float move, bool jump)
	{
		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}

	private void Update()
	{
		if (m_Rigidbody2D.velocity.magnitude > walkingParticleThreshold.magnitude && !walkingParticles.isPlaying)
		{
			walkingParticles.Play();
		} 
		if (m_Rigidbody2D.velocity.magnitude < walkingParticleThreshold.magnitude && walkingParticles.isPlaying)
		{
			walkingParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		}
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}