using Godot;
using System;

public class PlantProjectile : KinematicBody2D
{
	// Constants
	[Export] private int MAXSPEED = 250;
	private Vector2 velocity = new Vector2(0,0);
	
	public float theta = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		velocity.x = (float)Math.Cos(theta);
		velocity.y = (float)-Math.Sin(theta);
		velocity *= MAXSPEED;
		GD.Print("I exist!");
		GD.Print(Position);
	}
	
	public override void _PhysicsProcess(float delta)
	{
		MoveAndSlide(velocity);
	}
	
	// Signals
	private void OnHitboxAttackAreaEntered(object area)
	{
		GD.Print("Destroyed!");
		QueueFree();
	}
}

