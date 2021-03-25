using Godot;
using System;

public class PlantProjectileHitbox : Area2D
{
	// Plant
	PlantProjectile plantProjectile = null;
	
	// Signals
	[Signal] public delegate void area_entered();

	// Called when the node enters the scene tree for the first time. Connects
	// the signal of a collision onto this hitbox to the plant projectile so
	// that it can destroy itself.
	//
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		plantProjectile = this.Owner as PlantProjectile;
		Connect("area_entered", plantProjectile, "OnHitboxAttackAreaEntered");
	}
}
