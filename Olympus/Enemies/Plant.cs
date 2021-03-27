using Godot;
using System;

public class Plant : StaticBody2D
{
	// Constants
	[Export] private int SHOOTFRAMES = 50;

	// Variables
	[Export] private int timer = 0;
	PackedScene projectile = null;
	private bool isShooting = false;
	
	// Animation
	private AnimatedSprite plantAnimation;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		projectile = GD.Load<PackedScene>("res://Enemies/PlantProjectile.tscn");
		plantAnimation = GetNode<AnimatedSprite>("Plant"); 
		plantAnimation.Connect("animation_finished", this, "OnAnimationFinished");
	}
	
	// 
	public override void _PhysicsProcess(float delta)
	{
		if (timer == 0)
		{
			isShooting = true;
		} 
		if (isShooting)
		{
			plantAnimation.Play("Shoot");
		}
		else
		{
			plantAnimation.Stop();
		}
		timer++;
		timer %= SHOOTFRAMES;
	}
	
	private void OnHurtboxBodyAreaEntered(object area)
	{
		GD.Print("Plant: I am hurt!");
	}
	
	private void OnAnimationFinished()
	{
		PlantProjectile Projectile = (PlantProjectile)projectile.Instance();
		Projectile.RotationDegrees = RotationDegrees + 90;
		Projectile.Position = new Vector2(0,-10);
		Projectile.theta = 1.57f;
		AddChild(Projectile);
		isShooting = false;
	}

	
}

