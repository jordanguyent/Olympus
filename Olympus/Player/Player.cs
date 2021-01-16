using Godot;
using System;

public class Player : KinematicBody2D
{
	// Declare member variables here. Examples:
	[Export] public int MAXSPEED = 100;
	[Export] int ACCELERATION = 1000;
	// [Export] int FRICTION = 300;
	[Export] int JUMPSPEED = -250;
	[Export] int GRAVITY = 900;
	[Export] int MAXJUMPFRAME = 7;
	
	private Vector2 velocity = new Vector2(0,0);
	bool isPressingJump = false;
	int jumpFrame = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("I am in the game now :)");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		// Remember the current y velocity, calculate the new x velocity
		// ignoring the y velocity, then re-apply y velocity. This method is
		// used to implement acceleration for x direction. 
		float y = velocity.y + delta * GRAVITY;
		float x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		velocity = velocity.MoveToward(new Vector2(x, 0) * MAXSPEED, delta * ACCELERATION);
		velocity.y = y;
		
		// IsActionJustPressed used instead of IsActionPressed because truth 
		// value only changes for one frame, so you cant just hold up to keep
		// jumping. Implemented shorthop-fullhop with if/if else/else statement.
		// Player can hold a jump for 0-JUMPMAXFRAME frames.
		if ( Input.IsActionJustPressed("ui_up") && (IsOnFloor() || IsOnWall()) )
		{
			velocity.y = JUMPSPEED;
			jumpFrame++;
			isPressingJump = true;
		}
		else if (isPressingJump && jumpFrame < MAXJUMPFRAME && Input.IsActionPressed("ui_up"))
		{
			velocity.y = JUMPSPEED;
			jumpFrame++;
		}
		else
		{
			jumpFrame = 0;
			isPressingJump = false;
		}
		
		// MoveAndSlide takes a velocity vector and an "up direction" vector. 
		// up direction vector used by IsOnFloor to determine if the the player
		// is on floor. Also stops y velocity from becoming extremely negative
		// when standing on the floor. Basically black magic.
		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
	}
}
