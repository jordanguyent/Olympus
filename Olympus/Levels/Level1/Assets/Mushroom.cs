using Godot;
using System;

public class Mushroom : StaticBody2D
{
	// Variables
	public MushroomEffect effect = null;

	// The only purpose of this class is so that we may change the variables
	// theta in the effect box which will shoot the player in different 
	// angles. (tldr) just sets the theta for the effect box with respect to the
	// world.
	//
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		effect = GetNode<MushroomEffect>("Effectbox");
		effect.theta = (RotationDegrees + 90) * (float) Math.PI / 180;
	}
}
