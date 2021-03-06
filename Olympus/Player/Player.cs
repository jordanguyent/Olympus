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
// * why does the ball follow the player and is not independent anymore?
// * make it so that the ball does not collide with the player when player is
//		throwing the ball. Basically, the ball should only begin to bounce off
//		the player after it has collided with one other thing.
// * make it so that the player takes some of the momentum and bounces off of
// 		the ball. (user more signals)
// * 
// * clean things in the code. see todo and warnings
// 
// * build a sample level
// * animations (later)

// is there anything that should be commented here?
public class Player : KinematicBody2D
{
	// Movement Constants
	[Export] int MAXSPEEDX = 100;
	[Export] int MAXSPEEDY = 300;
	[Export] int ACCELERATION = 1000;
	[Export] int GRAVITY = 900;
	[Export] int JUMPSPEED = -200;
	[Export] float ONWALLFALLSPEED = 100;
	[Export] float WALLJUMPFACTORX = 1.4f;
	[Export] float WALLJUMPFACTORY = 1.2f;
	[Export] int DEFAULTDASHCOUNT = 1;
	[Export] int DASHSPEED = 300;
	private Vector2 E1 = new Vector2(1, 0);
	private Vector2 E2 = new Vector2(0, 1);

	// Frame Data Constants
	[Export] int MAXJUMPFRAME = 10;
	[Export] int FRAMELOCKXY = 5;
	[Export] int INPUTBUFFERMAX = 5;
	[Export] int DASHFRAMELOCKXY = 10;

	// Object Constants
	
	// Player Movement Variables
	private Vector2 velocity = new Vector2(0,0);
	private Vector2 userInput = new Vector2(0,0);
	private bool lastOnFloor = false;
	private bool justPressedJump = false;
	private bool isFastFalling = false;
	private bool isDashing = false;
	private int lastCollisionDirectionX = 1;
	private int lastCollisionDirectionY = 1;
	private int lastFacingDirection = 1;
	private int frameLockX = 0;
	private int frameLockY = 0;
	private int jumpFrames = 0;
	private int inputBufferFrames = 0;
	private int dashCount = 0;
	
	// Animation Variables
	private AnimatedSprite playerAnimation;
	// [TODO] Try animationTree and Player since it provides more functionality. 
	// Try both methods, see what works best
	
	//  Called when the node enters the scene tree for the first time.
	// Called when the node enters the scene tree for the first time. Used to
	// connect signals from Player to its child nodes like Ball.
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//   
	public override void _Ready() 
	{ 
		// Retrieves the Player's AnimatedSprite node in order to call its 
		// methods.
		playerAnimation = GetNode<AnimatedSprite>("AnimatedSprite");
		playerAnimation.Play("Idle");
	}

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
		// Update player based on user inputs so we may make calculations
		// about their movement consistently in the following functions.
		HelperUpdatePlayerState();
		
		// Calculation of Player movements
		HelperUpdateVelocityX(delta);
		HelperUpdateVelocityY(delta);
		HelperUpdateVelocityOnJump(justPressedJump);
		
		// MoveAndSlide takes a velocity vector and an "up direction" vector to
		// know in what direction the floor is. This info is necessary for the
		// function IsOnFloor to work properly. Move and slide already takes
		// delta into account. 
		velocity = MoveAndSlide(velocity, -1 * E2);

		// Plays correct animation
		PlayAnimation();
	}

	// Supposed to record the inputs from the user at the start of each fram so
	// that we can consistently calculate what the player is doing
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//
	private void HelperUpdatePlayerState()
	{
		// Freeze user inputs to be this for the rest of the calculations as it
		// is possible for these values to change between lines of code and 
		// result in inconcistencies in code. 
		userInput.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		userInput.y = Input.GetActionStrength("ui_up");

		// Update last facing direction accordingly
		lastFacingDirection = (userInput.x == 0) ? lastFacingDirection : Math.Sign(userInput.x);

		// Dashing is implemented here.
		// [ CONSIDER ] Should player be able to dash IMMEDIATELY (next frame) after a wall jump?
		// currently, right after a wall jump, we set frameLocks for XY so we can't do it, BUT if
		// we wanted to, we can have a dashLock variable and check for it here instead. 
		if (frameLockX == 0)
		{
			// Dash condition: just pressed dash button (not held) and we have
			// another dash avaliable. We only check this when frameLockX is
			// frameLockX should prevent any horizontal movement changes. We
			// also set frameLockY and 0 velocity.y so that the player can
			// "float" while dashing.
			GD.Print("DASHING");
			if (Input.IsActionJustPressed("ui_dash") && dashCount > 0)
			{
				GD.Print("DASHING");
				isDashing = true;
				dashCount--;
				frameLockX = DASHFRAMELOCKXY;
				frameLockY = DASHFRAMELOCKXY;
				velocity.x = DASHSPEED * lastFacingDirection;
				velocity.y = 0;
			}
			else
			{
				isDashing = false;
			}
			// Dashes reset only when on the ground and player is not dashing.
			if (IsOnFloor() && !isDashing)
			{
				dashCount = DEFAULTDASHCOUNT;
			}
		}

		// This code here is for buffering a jump. 
		if (Input.IsActionJustPressed("ui_up"))
		{
			justPressedJump = true;
			inputBufferFrames = INPUTBUFFERMAX;
		}
		if (inputBufferFrames == 0)
		{
			justPressedJump = false;
		}
		if (justPressedJump)
		{
			inputBufferFrames--;
		}

		// This code here is for updating the last thing that the player 
		// collided with in both the x and y directions. This information is
		// important for jumping off walls.
		for (int i = 0; i < GetSlideCount(); i++)
		{
			Vector2 collisionVector = GetSlideCollision(i).Normal;
			lastCollisionDirectionX = collisionVector.x != 0 ? -Math.Sign(collisionVector.x) : lastCollisionDirectionX;
			lastCollisionDirectionY = collisionVector.y != 0 ? -Math.Sign(collisionVector.y) : lastCollisionDirectionY;
		}
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
		else
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
		// If we currently have a frame lock for Y we ignore the user's input
		// and just "keep" the current momentum for the player so that he may
		// not counter his velocity during a wall jump. 
		if (frameLockY == 0)
		{
			// We want a "friction" like thing when player is on a wall, but 
			// only if he is already falling not when he is on the way up.
			if (IsOnWall() && velocity.y > 0)
			{
				velocity.y = HelperMoveToward(velocity.y, ONWALLFALLSPEED, delta * GRAVITY);
			}
			// If we are just in the air we want gravity to be applied until 
			// the player reaches their terminal velocity, MAXSPEEDY. If the
			// player is fast falling, fall factor is > 1 else 1. the player
			// will also decelerate to MAXSPEEDY as fast as they accelerate
			// to their fast fall speed (see HelperUpdatePlayerState)
			else
			{
				velocity.y = HelperMoveToward(velocity.y, MAXSPEEDY, delta * GRAVITY);
			}
		}
		// If the frame lock for Y is nonzero, decrement it until it is 0 so
		// that the player may be allowed to move again.
		else
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
		// [ CONSIDER ] making the || into an && since jumping kinda requires
		// both directions right? 

		// jumping is only possible if there is an inactive frameLock.
		if (frameLockX == 0 || frameLockY == 0)
		{
			// The start of a jump
			if (justPressedJump)
			{
				// When player is on floor, just adjust velocity.y. jumpFrames
				// set to MAXJUMPFRAME to enable variable height jump. Remember
				// that the player was last on floor for the vairbale jump.
				if (IsOnFloor())
				{
					velocity.y = JUMPSPEED;
					jumpFrames = MAXJUMPFRAME;
					lastOnFloor = true;
					inputBufferFrames = 0;
				}
				// When player is on wall, use direction of last collision to
				// jump away from wall and update velocity vector. Set frame
				// lock x so that user cant go against own wall jump momentum.
				// Also, lastCollisionDirectionX is a nonzero value.
				else if (IsOnWall())
				{
					velocity.x = Math.Sign(lastCollisionDirectionX) * -WALLJUMPFACTORX * MAXSPEEDX;
					velocity.y = JUMPSPEED * WALLJUMPFACTORY;
					frameLockX = FRAMELOCKXY;
					jumpFrames = 0;
					lastOnFloor = false;
					inputBufferFrames = 0;
				}
			}
			// User is holding down the jump button after starting a jump. This
			// is to get variable jump heights. 
			if (userInput.y != 0 && jumpFrames != 0)
			{
				// Implementation of the variable jump height.
				if (lastOnFloor)
				{
					jumpFrames--;
					velocity.y = JUMPSPEED;
				}
				// If hits the ceiling, act like user has hit apex of jump to
				// not slide through corners. We want this to be the case if
				// jumping off wall or ground.
				if (IsOnCeiling())
				{
					jumpFrames = 0;
				}
			} 
			// This is done to ensure user jumpFrame goes back to 0 in the case
			// that they do a short hop since we dont want them to be able to
			// jump for first two frames, then stop for another two frames, and
			// then continue jumping afterwards the remaining jumpFrames.
			else 
			{
				jumpFrames = 0;
			}
		}
	}
	
	// Helper function to do "linear introperlation" without having to caluclate
	// it automatically and just make it depend on a known working function.
	private float HelperMoveToward(float current, float desire, float acceleration)
	{
		return (E1 * current).MoveToward(E1 * desire, acceleration).x;
	}
	
	// Plays the correct animation
	private void PlayAnimation()
	{
		// determines where player faces
		if (velocity.x < 0)
			playerAnimation.FlipH = true;
		else if (velocity.x > 0)
			playerAnimation.FlipH = false;

		// Running
		if (IsOnFloor())
		{
			if (velocity.x != 0)
				playerAnimation.Play("Run");
			else
				playerAnimation.Play("Idle");
		} 
		// Jump
		else 
		{	
			if (velocity.y < 0)
				playerAnimation.Play("Jump0");
			else if (velocity.y > 0)
				if (IsOnWall())
					playerAnimation.Play("Climb");
				else
					playerAnimation.Play("Jump2");
				
		}
		
		// Dash
		if (isDashing)
		{
			playerAnimation.Play("Dash");
		}
	}
}



