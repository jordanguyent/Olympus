using Godot;
using System;


// [ TODO ] Search up how to set up ball for player
// [ TODO ] Search up how to structure the ball (kinematic/static/ridged/else body 2D ???)
// [ TODO ] Fix collision with ball and player

public class Ball : KinematicBody2D
{
	// Ball Constants
	[Export] private int SPEED = 100;

	// Ball Variables
	private Vector2 momentum = new Vector2(0,0);
	private Vector2 direction = new Vector2(0,0);
	private bool canCollideWithThrower = false;
	// private Node Thrower = null;

	// Ball Signals
	// [Signal] 

	// Called when the node enters the scene tree for the first time.
	// 
	// Parameters
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready() {}

	// Recalculates the trajectory of the ball. It will bounce off of surfaces
	// but not always on players (hopefully)
	//
	// Parameters 
	// ----------
	// delta : time elapsed since previous frame
	//
	// Returns
	// -------
	//   
	public override void _PhysicsProcess(float delta)
	{
		momentum = SPEED * direction;
		MoveAndSlide(momentum);
		direction = (GetSlideCount() > 0) ? direction.Bounce(GetSlideCollision(0).Normal) : direction;
	}

	// Recieves the Signal that it has been thrown
	//
	// Parameters
	// ----------
	// normalizedDirection : 
	// 
	// Returns
	// -------
	// 
	public void GetThrowSignal(Vector2 normalizedDirection, Vector2 position)
	{
		Position = position;
		direction = normalizedDirection;
	}

	// Signal that is recieved when the ball leaves the body of the thrower. 
	// Makes canCollideWithPlayer so that the ball may now collide with the
	// thrower.
	// 
	// Parameters
	// ----------
	//
	// Returns
	// -------
	//
	public void GetLeavingBodySignal()
	{
		canCollideWithThrower = true;
	}

	// Signal that is recieved when the ball first enters the body of the 
	// thrower. If the ball can collide with the thrower, it will collide
	// and transfer some momentum to the thrower.
	// 
	// Parameters
	// ----------
	//
	// Returns
	// -------
	//
	public void GetCollisionWithEntitySignal()
	{
		// if canCollideWithThrower
		//      will bouce and give player momentum
		//      send signal to player to change its velocity
	}
}
