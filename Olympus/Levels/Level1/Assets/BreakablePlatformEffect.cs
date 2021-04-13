using Godot;
using System;

public class BreakablePlatformEffect : Area2D
{
	// Variables
	[Export] int BDELAY = 100;
	[Export] int SDELAY = 150;
	int bTimer;
	int sTimer;
	CollisionShape2D collision = null;
	bool inArea = false;
	bool isBreaking = false;
	bool isRespawning = false;
	String currentAnimation = null;

	// Objects
	AnimatedSprite animatedSprite = null;
	
	// Signals
	[Signal] public delegate void area_entered();
	[Signal] public delegate void area_exited();

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
		bTimer = BDELAY;
		sTimer = SDELAY;
		collision = this.Owner.GetNode<CollisionShape2D>("CollisionShape2D");
		animatedSprite = this.Owner.GetNode<AnimatedSprite>("AnimatedSprite");

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
		// Block is breaking
		if (isBreaking) 
		{
			bTimer--;
			animatedSprite.Play("Breaking");
		}

		// Block is Falling and Broken. Block goes into respawning state.
		if (bTimer <= 0 && sTimer > 0)
		{
			isBreaking = false;
			isRespawning = true;
			collision.Disabled = true;
			sTimer--;
			animatedSprite.Play("Broken");
		}

		// Block is respawning, reset values. Returns to Idle state after 
		// animation finishes
		if (sTimer <= 0 && !inArea)
		{
			collision.Disabled = false;
			sTimer = SDELAY;
			bTimer = BDELAY;
			isRespawning = false;
			animatedSprite.Play("Respawning");
		}

		currentAnimation = animatedSprite.Animation;
	}

	// When the box detects a collision with something it will send itself a 
	// signal to emit another signal to the player making them bounce. I had a
	// lot of trouble getting it done any other way.
	//
	// Issues: What if we want mobile enemies to interact with the mushroom?
	// Solution: We need all enemies to inherit from the same "enemy" node so
	// that we may use polymorphism to get around this.
	
	
	// When the box detects a collision with something it will send itself a 
	// signal to emit another signal to the player making them bounce. I had a
	// lot of trouble getting it done any other way.
	//
	// Issues: What if we want mobile enemies to interact with the mushroom?
	// Solution: We need all enemies to inherit from the same "enemy" node so
	// that we may use polymorphism to get around this.
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
	
	private void OnAnimationFinished()
	{
		if (currentAnimation == "Broken")
		{
			animatedSprite.Stop();
		} 
		else if (currentAnimation == "Respawning")
		{
			animatedSprite.Play("Idle");
		}
		
	}
}
