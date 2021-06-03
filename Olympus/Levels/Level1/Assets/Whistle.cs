using Godot;
using System;

public class Whistle : StaticBody2D
{

	// Note, what happens when change state without resetting timers? Make sure
	// to reset Bird Timer and Player Timers

	// Signals
	[Signal] public delegate void area_entered();
	[Signal] public delegate void change_state();

	// state
	enum WhistleState 
	{
		Ready,
		Used,
		Cooldown,
		Respawn
	}

	// Whistle Variables
	private WhistleState state = WhistleState.Ready;
	private const int RESPAWNDELAY = 300;
	private int respawnTimer;
	private bool hasCollided = false;

	// Nodes
	private Player player = null;
	private Bird bird = null;
	private Area2D effectBox = null;
	private AnimatedSprite whistleAnimation = null;

	// Called when the node enters the scene tree for the first time.
	// Called when the node enters the scene tree for the first time. Used to
	// connect signals from Whistle and initialize objects
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		// Get Nodes
		player = GetNode<Player>("../Player");
		bird = GetNode<Bird>("../Bird");
		effectBox = GetNode<Area2D>("Effectbox");
		whistleAnimation = GetNode<AnimatedSprite>("AnimatedSprite");

		// Error Checking
		if (player == null)
			throw new ArgumentNullException("Whistle.cs: Player not found!"); 
		if (bird == null)
			throw new ArgumentNullException("Whistle.cs: Bird not found!"); 

		// Connect Signals
		effectBox.Connect("area_entered", this, "OnAreaEntered");
		Connect("change_state", bird, "RecallBird");

		// Initialize Variables
		respawnTimer = RESPAWNDELAY;

	}

	// Finite State Machine for whistle transitions, actions, and animations.
	//
	// Parameters 
	// ----------
	// delta : time elapsed since previous frame
	//
	// Returns
	// -------
	//  
	public override void _PhysicsProcess(float delta)
	{

		// State machine
		// ======================
		// Ready: Used
		// Used: Cooldown
		// Cooldown: Respawn
		// Respawn: Ready
		// ======================

		// Transitions
		switch (state)
		{
			case WhistleState.Ready:
				if (hasCollided)
				{
					state = WhistleState.Used;
					hasCollided = false;
				}
				break;

			case WhistleState.Used:
				state = WhistleState.Cooldown;
				break;

			case WhistleState.Cooldown:
				respawnTimer--;
				if (respawnTimer <= 0)
				{
					state = WhistleState.Respawn;
					respawnTimer = RESPAWNDELAY;
				}
				break;

			case WhistleState.Respawn:
				// Add animation for respawn here
				state = WhistleState.Ready;
				break;

			default:
				throw new ArgumentNullException("Whistle.cs: Not a valid WhistleState"); 
		}

		// Actions
		switch (state)
		{
			case WhistleState.Ready:
				effectBox.SetCollisionMaskBit(5, true);
				break;

			case WhistleState.Used:
				EmitSignal("change_state");
				effectBox.SetCollisionMaskBit(5, false);
				break;

			case WhistleState.Cooldown:
				break;

			case WhistleState.Respawn:
				break;

			default:
				throw new ArgumentNullException("Whistle.cs: Not a valid WhistleState"); 
		}

		// Animations
		switch (state)
		{
			case WhistleState.Ready:
				whistleAnimation.Modulate = new Color(1, 1, 1, 1);
				break;

			case WhistleState.Used:
				whistleAnimation.Modulate = new Color(0, 0, 0, 0.2f);
				break;

			case WhistleState.Cooldown:
				break;

			case WhistleState.Respawn:
				break;

			default:
				throw new ArgumentNullException("Whistle.cs: Not a valid WhistleState"); 
		}
	}

	// Signal:
	// Checks if player has entered the area
	//
	// Parameters 
	// ----------
	// area : area2d that this collided with
	//
	// Returns
	// -------
	//  
	private void OnAreaEntered(object area)
	{
		hasCollided = true;
	}
}
