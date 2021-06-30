using Godot;
using System;

public class TrapPlant : StaticBody2D
{

	[Signal] public delegate void area_entered();

	// state
	enum PlantState 
	{
		Ready,
		Triggered,
		Attack,
		Reset
	}

	// TrapPlant variables
	private PlantState state = PlantState.Ready;
	private const int ATTACKDELAY = 90;
	private int attackTimer;
	private const int RESETDELAY = 120;
	private int resetTimer;
	private bool hasEntered = false;

	// Nodes
	private Player player = null;
	private AnimatedSprite plantAnimation = null;
	private Area2D triggerBox = null;
	private Area2D collisionBox = null;

	// Called when the node enters the scene tree for the first time.
	// Called when the node enters the scene tree for the first time. Used to
	// connect signals from TrapPlant and initialize objects
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	// 
	public override void _Ready() 
	{
		// Obtain nodes
		player = GetNode<Player>("../Player");
		plantAnimation = GetNode<AnimatedSprite>("AnimatedSprite");
		triggerBox = GetNode<Area2D>("TriggerBox");
		collisionBox = GetNode<Area2D>("CollisionBox");

		// Error Checking
		if (player == null)
			throw new ArgumentNullException("TrapPlant.cs: Player not found!"); 

		// Connecting Signals
		triggerBox.Connect("area_entered", this, "OnTriggerBoxEntered");
		collisionBox.Connect("area_entered", player, "OnHurtboxBodyAreaEntered");

		// Variable initialization
		attackTimer = ATTACKDELAY;
		resetTimer = RESETDELAY;
	}

	// Finite State Machine for TrapPlant transitions, actions, and animations.
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
		// Ready: Triggered
		// Triggered: Attack
		// Attack: Reset
		// Reset: Ready
		// ======================

		// Transitions
		switch (state)
		{
			case PlantState.Ready:
				if (hasEntered) 
				{
					hasEntered = false;
					state = PlantState.Triggered;
				}
				break;

			case PlantState.Triggered:
				attackTimer--;
				if (attackTimer <= 0)
				{
					attackTimer = ATTACKDELAY;
					state = PlantState.Attack;
				}
				break;

			case PlantState.Attack:
				state = PlantState.Reset;
				break;

			case PlantState.Reset:
				resetTimer--;
				if (resetTimer <= 0)
				{
					resetTimer = RESETDELAY;
					state = PlantState.Ready;
				}
				break;
			
			default:
				throw new ArgumentNullException("TrapPlant.cs: Not a valid PlantState"); 
		}

		// Actions
		switch (state)
		{
			case PlantState.Ready:
				collisionBox.SetCollisionMaskBit(5, false);
				triggerBox.SetCollisionMaskBit(5, true);
				break;

			case PlantState.Triggered:
				triggerBox.SetCollisionMaskBit(5, false);
				break;

			case PlantState.Attack:
				collisionBox.SetCollisionMaskBit(5, true);
				break;

			case PlantState.Reset:
				break;
			
			default:
				throw new ArgumentNullException("TrapPlant.cs: Not a valid PlantState"); 
		}

		// Animations
		switch (state)
		{
			case PlantState.Ready:
				plantAnimation.Position = new Vector2(0, -4);
				plantAnimation.Scale = new Vector2(8, 2);
				plantAnimation.Modulate = new Color(1, 1, 1, 1);
				break;

			case PlantState.Triggered:
				plantAnimation.Modulate = new Color(1, 0.5f, 0, 1);
				break;

			case PlantState.Attack:
				plantAnimation.Position = new Vector2(0, -8);
				plantAnimation.Scale = new Vector2(4, 4);
				plantAnimation.Modulate = new Color(1, 0, 0, 1);
				break;

			case PlantState.Reset:
				plantAnimation.Modulate = new Color(1, 1, 1, 0.5f);
				break;
			
			default:
				throw new ArgumentNullException("TrapPlant.cs: Not a valid PlantState"); 
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
	private void OnTriggerBoxEntered(object area)
	{
		hasEntered = true;
	}
}
