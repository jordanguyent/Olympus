using Godot;
using System;

public class MushroomEffect : Area2D
{
	// [ TODO ] : make this reference the player directly using a directory so
	// that we dont have to do the sketchy stuff in ready.
	// Player
	Player player = null;
	AnimatedSprite animatedSprite = null;
	
	// Signals
	[Signal] public delegate void area_entered();
	[Signal] public delegate void Bounce();

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
		player = this.Owner.Owner.GetNode<Player>("Player");
		animatedSprite = this.Owner.GetNode<AnimatedSprite>("AnimatedSprite");
		Connect("area_entered", this, "OnEffectboxAreaEntered"); // Ok connect
		Connect("Bounce", player, "OnFixedBounceableAreaEntered"); // Ok connect
		animatedSprite.Connect("animation_finished", this, "OnAnimationFinished");
	}
	
	// When the box detects a collision with something it will send itself a 
	// signal to emit another signal to the player making them bounce. I had a
	// lot of trouble getting it done any other way.
	//
	// Issues: What if we want mobile enemies to interact with the mushroom?
	// Solution: We need all enemies to inherit from the same "enemy" node so
	// that we may use polymorphism to get around this.
	private void OnEffectboxAreaEntered(object area)
	{
		GD.Print("Effectbox now emitting bounce signal!");
		animatedSprite.Play("Bounce");
		EmitSignal("Bounce");
	}

	private void OnAnimationFinished()
	{
		animatedSprite.Stop();
	}
}

