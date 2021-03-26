using Godot;
using System;

public class MushroomEffect : Area2D
{
	// Player
	Player player = null;
	
	// Signals
	// [Signal] public delegate void area_entered();

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
		player = this.Owner.Owner.GetChildren()[2] as Player;
		Connect("area_entered_bounce", player, "OnFixedBounceableAreaEntered");
	}
}
