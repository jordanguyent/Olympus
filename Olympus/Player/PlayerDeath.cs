using Godot;
using System;

public class PlayerDeath : AnimatedSprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect("animation_finished", this, "OnAnimationFinish");
		Frame = 0;
		Play("Death");
	}
	
	// Destroys the instance when animation is finished
	// Connected through a signal in _Ready()
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//
	private void OnAnimationFinish()
	{
		QueueFree();
		GetTree().ReloadCurrentScene();
	}
}
