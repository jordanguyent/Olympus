using Godot;
using System;

// DATE CREATED  : JAN 2020
// AUTHOR(S)     : ALEXIS CHAVOYA, EDWIN ZHONG, JORDAN NGUYEN
// 
// KEY WORDS IN COMMENTARY
// -----------------------
// [ TODO ]     : things to do/add
// [ CONSIDER ] : discussions for future meets
// [ WARNING ]  : issues with code
// 
// GOALS FOR FUTURE MEETS 
// ----------------------
// * add an input buffer
// * clean things in the code. see todo and warnings
// * more consistent code, sometimes we count up to a value like with jumpFrame
//		and other times we count down like with frameLockX/Y. 

public class Player : KinematicBody2D
{
	// Movement Constants
	[Export] int MAXSPEEDX = 100;
	[Export] int MAXSPEEDY = 300;
	[Export] int ACCELERATION = 1000;
	[Export] int GRAVITY = 900;
	[Export] int JUMPSPEED = -200;
	[Export] float FALLRATE = 100;
	[Export] float WALLJUMPFACTORX = 1.4f;
	[Export] float WALLJUMPFACTORY = 1.2f;
	private Vector2 E1 = new Vector2(1, 0);
	private Vector2 E2 = new Vector2(0, 1);

	// Frame Data Constants
	[Export] int MAXJUMPFRAME = 10;
	[Export] int FRAMELOCKXY = 5;
	
	// Player Movement Variables
	private Vector2 velocity = new Vector2(0,0);
	private Vector2 userInput = new Vector2(0,0);
	private int jumpFrame = 0;
	private int frameLockX = 0;
	private int frameLockY = 0;
	private bool lastOnFloor = false;
	
	//  Called when the node enters the scene tree for the first time.
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//   
	public override void _Ready() {}

	// Obtains information about user input and uses information to calculate
	// and update velocty as well as move the player in-game.
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
		// [ TODO ] create an updatePlayerSate() function here that updates 
		// values like userInput, lastOnFloor, and other booleans that are used
		// to determine what the player should be doing.

		// Freeze user inputs to be this for the rest of the calculations as it
		// is possible for these values to change between lines of code and 
		// result in inconcistencies in code. 
		userInput.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		userInput.y = Input.GetActionStrength("ui_up");
		bool justPressedJump = Input.IsActionJustPressed("ui_up");
		
		// Calculation of Player movements
		HelperUpdateVelocityX(delta);
		HelperUpdateVelocityY(delta);
		HelperUpdateVelocityOnJump(justPressedJump);
		
		// MoveAndSlide takes a velocity vector and an "up direction" vector to
		// know in what direction the floor is. This info is necessary for the
		// function IsOnFloor to work properly.
		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
	}

	// Calculates and updates velocity.x
	//
	// Parameters 
	// ----------
	// delta : time elapsed since previous frame
	//
	// Returns
	// -------
	//   
	private void HelperUpdateVelocityX(float delta)
	{
		// If we currently have a frame lock for X we ignore the user's input
		// and just "keep" the current momentum for the player so that he may
		// not counter his velocity during a wall jump. 
		if (frameLockX == 0)
		{
			velocity.x = HelperMoveToward(velocity.x, userInput.x * MAXSPEEDX, delta * ACCELERATION);
		}
		// If the frame lock for X is nonzero, decrement it until it is 0 so
		// that the player may be allowed to move again.
		else if (frameLockX > 0)
		{
			frameLockX--;
		}
	}

	// Calculates and updates velocity.y
	//
	// Parameters 
	// ----------
	// delta : time elapsed since previous frame
	//
	// Returns
	// -------
	//   
	private void HelperUpdateVelocityY(float delta)
	{
		// [ WARNING ] implementing the frameLockY here makes it so that the 
		// player always gets a "long-jump" in the air. Consider removing 
		// frameLockY to allow gravity imediately after jump and increase 
		// WALLJUMPFACTORY instead for stability in case we need to use 
		// frameLockY for other reasons.
		// [ CONSIDER ] currently acceleration to fall rate is just gravity, 
		// maybe we want it to be something else (a factor of gravity maybe)?
		// [ CONSIDER ] maybe we want some kind of fast fall like thing?

		// If we currently have a frame lock for Y we ignore the user's input
		// and just "keep" the current momentum for the player so that he may
		// not counter his velocity during a wall jump. 
		if (frameLockY == 0)
		{
			// We want a "friction" like thing when player is on a wall, but 
			// only if he is already falling not when he is on the way up.
			if (IsOnWall() && velocity.y > 0)
			{
				velocity.y = HelperMoveToward(velocity.y, FALLRATE, delta * GRAVITY);
			}
			// If we are just in the air we want gravity to be applied until 
			// the player reaches their terminal velocity, MAXSPEEDY.
			else
			{
				velocity.y = HelperMoveToward(velocity.y, MAXSPEEDY, delta * GRAVITY);
			}
		}
		// If the frame lock for Y is nonzero, decrement it until it is 0 so
		// that the player may be allowed to move again.
		else if (frameLockY > 0)
		{
			frameLockY--;
		}
	}

	// Recalculates the velocity vector on player jump depending on if the 
	// player is on the ground or a wall. Jumping off a wall will result in
	// frameLockX/Y being set.
	// 
	// Parameters 
	// ----------
	// jumpPressedJump : truth if player just pressed jump this frame
	// 
	// Returns
	// -------
	// 
	private void HelperUpdateVelocityOnJump(bool justPressedJump)
	{
		// [ WARNING ] GetSlideCollision(), count from GetSlideCount() - 1 to 0
		// and stop at the first instance of a nonzero number. make a while loop
		// instead of a for loop. in addition, for a better optimization still,
		// when getting the userInput, update a variable about last collision
		// that is only updated for nonzero collisions.

		// jumping is only possible if there is an inactive frameLock. (why???)
		if (frameLockX == 0 || frameLockY == 0)
		{
			// The start of a jump
			if (justPressedJump)
			{
				// When player is on floor, just adjust velocity.y. jumpFrame
				// reset to 0 to enable variable height jump. Remember that
				// the player was last on floor since (???)
				if (IsOnFloor())
				{
					velocity.y = JUMPSPEED;
					jumpFrame = 0;
					lastOnFloor = true;
				}
				// When player is on wall, get the direction of last collision,
				// and update velocity vector. Set frame locks so that user
				// cant go against their wall jump momentum. 
				else if (IsOnWall())
				{

					// getting direction of which player collided
					int direction = 1;
					for (int i = 0; i < GetSlideCount(); i++)
					{
						var collision = GetSlideCollision(i);
						if (collision.Normal.x > 0) // left
						{
							direction = -1;
						}
						else if (collision.Normal.x < 0) // right
						{
							direction = 1;
						}
					}
					velocity.x = Math.Sign(direction) * -WALLJUMPFACTORX * MAXSPEEDX;
					velocity.y = JUMPSPEED * WALLJUMPFACTORY;
					frameLockX = FRAMELOCKXY;
					frameLockY = FRAMELOCKXY;
					jumpFrame = MAXJUMPFRAME;
					lastOnFloor = false;
				}
			}
			// User is holding down the jump button after starting a jump. This
			// is to get variable jump heights. 
			if (userInput.y != 0 && jumpFrame < MAXJUMPFRAME)
			{
				// If hits the ceiling, act like user has hit apex of jump to
				// not slide through corners. We want this to be the case if
				// jumping off wall or ground.
				if (IsOnCeiling())
				{
					jumpFrame = MAXJUMPFRAME;
				}
				// Implementation of the variable jump height.
				if (lastOnFloor)
				{
					jumpFrame++;
					velocity.y = JUMPSPEED;
				}
			} 
			// This is done to ensure user jumpFrame goes back to MAXJUMPFRAME
			// in the case that they do a short hop.
			else 
			{
				jumpFrame = MAXJUMPFRAME;
			}
		}
	}
	
	// Helper function to do "linear introperlation" without having to caluclate
	// it automatically and just make it depend on a known working function.
	private float HelperMoveToward(float current, float desire, float acceleration)
	{
		return (E1 * current).MoveToward(E1 * desire, acceleration).x;
	}
}



