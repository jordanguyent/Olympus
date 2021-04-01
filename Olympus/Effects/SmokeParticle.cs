using Godot;
using System;

public class SmokeParticle : AnimatedSprite
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect("animation_finished", this, "OnAnimationFinished");
		Play("Smoke");
	}

	// Signal - Plays whenever AnimatedSprite finishes
	//
	// Parameters 
	// ----------
	// 
	// Returns
	// -------
	//
	private void OnAnimationFinished()
	{
		QueueFree();
	}
}
