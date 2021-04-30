using Godot;
using System;

public class Zone_B : Area2D
{
	[Signal]
	public delegate void ZoneBEntered(string ZoneLetter);

	[Signal]
	public delegate void TransitionPlayer(String transitionAnimation);

	private ColorRect transitionRect = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		transitionRect = GetNode<ColorRect>("TransitionRect");
		SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
		Connect("ZoneBEntered", SCNHAND, "ChangeStage");
		Connect("TransitionPlayer", transitionRect, "PlayTransition");
	}

	public override void _Process(float delta)
	{
		var OverlappingBodies = this.GetOverlappingBodies();
		if(OverlappingBodies.Count > 0)
		{
			foreach(var body in OverlappingBodies)
			{
				if( (body as Player) != null)
				{
					// Emitting signal to run transition
					EmitSignal("TransitionPlayer", "Fade");
				}
			}
		}
	}

	private void AfterAnimationFinished() 
	{
		EmitSignal("ZoneBEntered", "B");
	}
}
