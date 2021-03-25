using Godot;
using System;

public class PlantHurtbox : Area2D
{
	// Plant
	Plant plant = null;
	
	// Signals
	[Signal] public delegate void area_entered();

	// Called when the node enters the scene tree for the first time. Connects
	// the signal of a collision onto this hitbox to the player so that it can
	// destroy itself.
	//
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		plant = this.Owner as Plant;
		Connect("area_entered", plant, "OnHurtboxBodyAreaEntered");
	}
}
