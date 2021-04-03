using Godot;
using System;

public class FallingBlock : KinematicBody2D
{

	[Export] int FALLDELAY = 50;
	[Export] int MAXFALLSPEED = 300;
	[Export] int ACCELERATION = 100;

	private Vector2 velocity = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public override void _PhysicsProcess(float delta)
	{
		velocity = velocity.MoveToward(new Vector2(0, MAXFALLSPEED), delta *ACCELERATION);
		velocity = MoveAndSlide(velocity, Vector2.Up);
		Position = Position.Snapped(Vector2.One);
	}
}
