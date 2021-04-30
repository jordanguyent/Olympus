using Godot;
using System;

public class TransitionRect : ColorRect
{
	[Signal]
	public delegate void EmitSignalToParent();

	private AnimationPlayer animationPlayer = null;
	private Node2D parent = null;

	public override void _Ready() 
	{
		SetAsToplevel(true);
		parent = GetNode<Node2D>("../");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Connect("animation_finished", this, "OnAnimationFinished");
		Connect("EmitSignalToParent", parent, "AfterAnimationFinished");
	}


	private void PlayTransition(String transitionAnimation)
	{
		animationPlayer.Play(transitionAnimation);		
	}

	private void OnAnimationFinished(String animName)
	{
		EmitSignal("EmitSignalToParent");
	}
}
