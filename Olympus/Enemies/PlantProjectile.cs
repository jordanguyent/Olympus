using Godot;
using System;

public class PlantProjectile : KinematicBody2D
{
	// Constants
	[Export] private int MAXSPEED = 200;
	
	// Variables
	private Vector2 velocity = new Vector2(0,0);
	public float theta = 0;
	private Player player = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Math.Cos Math.Sin only work on radians so we convert RotationDegrees
		// to radians first. We take -cos(theta) for x since the rotation is
		// clockwise instead of the conventional counter clockwise in math. We
		// take -sin(theta) since up is negative and down is positive movement.
		theta = RotationDegrees * (float)Math.PI / 180;
		velocity.x = (float)-Math.Cos(theta);
		velocity.y = (float)-Math.Sin(theta);
		velocity *= MAXSPEED;
	}
	
	public override void _PhysicsProcess(float delta)
	{
		MoveAndSlide(velocity);
	}
	
	// Signals
	private void OnHitboxAttackAreaEntered(object area)
	{
		QueueFree();
	}
	
	private void OnBodyEntered(object body)
	{
		if (body.GetType().ToString() != "Player")
		{
			QueueFree();
		}
		
	}
}
