using Godot;
using System;

public class BreakablePlatformEffect : Area2D
{
	// Variables
	String currentAnimation = null;
	[Export] int BDELAY = 100;
	[Export] int SDELAY = 150;
	private int bTimer;
	private int sTimer;
	private bool inArea = false;
	private bool isBreaking = false;
	private bool isRespawning = false;
	private bool isOn = true;
	private Vector2 spritePos;

	// Objects
	AnimatedSprite animatedSprite = null;
	CollisionShape2D collision = null;
	
	// Signals
	[Signal] public delegate void area_entered();
	[Signal] public delegate void area_exited();


	// Enum
	enum PlatformState 
	{
		Wait,
		Break,
		Fall,
		Respawn
	}

	PlatformState state = PlatformState.Wait;

	// Called when the node enters the scene tree for the first time. Connects
	// the signal of a collision onto this hitbox to the player so that it can
	// destroy itself.
	//
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		// Initializes timer
		bTimer = BDELAY;
		sTimer = SDELAY;

		// Retrieve node path
		collision = Owner.GetNode<CollisionShape2D>("CollisionShape2D");
		animatedSprite = Owner.GetNode<AnimatedSprite>("AnimatedSprite");

		// Initialize position
		spritePos = animatedSprite.Position;

		// Connect signals
		Connect("area_entered", this, "OnEffectboxAreaEntered");
		Connect("area_exited", this, "OnEffectboxAreaExited");
		animatedSprite.Connect("animation_finished", this, "OnAnimationFinished");
	}

	// Runs in sync with the physics of the game engine. Includes two timers: a
	// Break Timer and a Respawn timer. Break Timer is the time it takes for the
	// block to disappear. Respawn timer is the time it takes for the block to 
	// reappear. 
	//
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _PhysicsProcess(float delta)
	{
		// transitions
		switch (state)
		{
			case PlatformState.Wait:
				if (isBreaking)
				{
					state = PlatformState.Break;
					isOn = false;
				}
				break;

			case PlatformState.Break:
				bTimer--;
				if (bTimer <= 0 && sTimer > 0)
				{
					state = PlatformState.Fall;
					isBreaking = false;
					isRespawning = true;
					collision.Disabled = true;
				}
				break;

			case PlatformState.Fall:
				sTimer--;
				if (sTimer <= 0 && !inArea)
				{
					state = PlatformState.Respawn;
					sTimer = SDELAY;
					bTimer = BDELAY;
					collision.Disabled = false;
					isRespawning = false;
				}
				break;

			case PlatformState.Respawn:
				if (isOn)
				{
					state = PlatformState.Wait;
				}
				break;

		}

		// animations
		switch (state)
		{
			case PlatformState.Wait:
				animatedSprite.Play("Idle");
				break;

			case PlatformState.Break:
				animatedSprite.Position = new Vector2(spritePos.x + (int) GD.RandRange(-1.5, 1.5),spritePos.y + (int) GD.RandRange(-1.5, 1.5));
				animatedSprite.Play("Breaking");
				break;

			case PlatformState.Fall:
				animatedSprite.Position = spritePos;
				animatedSprite.Play("Broken");
				break;

			case PlatformState.Respawn:
				animatedSprite.Play("Respawning");
				break;

		}

		currentAnimation = animatedSprite.Animation;
	}
	
	// When player enters the hitbox, the block starts to break
	private void OnEffectboxAreaEntered(object area)
	{
		// Block goes into breaking state
		if (!isBreaking && !isRespawning) {
			isBreaking = true;
		}
		inArea = true;
	}
	
	// When player exits the hitbox, inArea allows the block to spawn when
	// player is not inside the block, avoiding any collsion bugs. 
	private void OnEffectboxAreaExited(object area)
	{
		inArea = false;
	}
	
	// Plays when animation is finished
	private void OnAnimationFinished()
	{
		if (currentAnimation == "Broken")
		{
			animatedSprite.Stop();
		} 
		else if (currentAnimation == "Respawning")
		{
			animatedSprite.Play("Idle");
			isOn = true;
		}
		
	}
}
