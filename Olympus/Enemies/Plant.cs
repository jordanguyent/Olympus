using Godot;
using System;

public class Plant : StaticBody2D
{
	// Declare member variables here. Examples:
	private int SHOOTFRAMES = 60;
	private int timer = 0;
	
	PackedScene projectile = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		projectile = GD.Load<PackedScene>("res://Enemies/PlantProjectile.tscn");
		GD.Print("i am plant");
		GD.Print(Position);
	}
	
	// 
	public override void _PhysicsProcess(float delta)
	{
		if (timer == 0)
		{
			PlantProjectile Projectile = (PlantProjectile)projectile.Instance();
			Projectile.Position = new Vector2(0,-10);
			// Projectile.RotationsDegrees = RotationsDegrees;
			Projectile.theta = 1.57f;
			AddChild(Projectile);
		}
		timer++;
		timer %= SHOOTFRAMES;
	}
	
	private void OnHurtboxBodyAreaEntered(object area)
	{
		GD.Print("Plant: I am hurt!");
	}

	
}

