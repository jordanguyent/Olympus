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
// CODE STYLE
// ----------
// - Try to comment "why" we do things, not just "what" we are doing.
// - Comments are not to exceed the 80 character line mark, like this sentence.
// - Keep the standard comment above every function.
// |
// - MyClass, MyFunction, myVariable, MYCONSTANT
// - Every variable should be initialized, at least to 0, null, etc.
// - } and { should take a line each and should be included for ALL if/if else/
// else/for/while/do/switch statments
// - always decrement frame counters to 0 instead of counting up
// 
// GOALS FOR FUTURE MEETS 
// ----------------------
// 

// All functions contained in this class appear as follows in the order:
// 
// public override void _Ready() 
// public override void _PhysicsProcess(float delta)
// private void HelperUpdatePlayerState()
// private void HelperUpdateVelocityX(float delta)
// private void HelperUpdateVelocityY(float delta)
// private void HelperUpdateVelocityOnJump(bool justPressedJump)
// private float HelperMoveToward(float current, float desire, float acceleration)
// private void PlayAnimation()
// private void PlayDeathAnimation()
// 
// private void OnClimbableAreaEntered(object area)
// private void OnClimbableAreaExited(object area)
// private void OnFixedBounceableAreaEntered(object area)
// private void OnHurtboxBodyAreaEntered(object area)
// 
public class Player : KinematicBody2D
{
	// Base world node
	World baseWorld = null;

	// Signals
	[Signal] public delegate void PlayerDeath();
	[Signal] public delegate void area_entered_bounce();

	// World Movement Constants
	[Export] int MAXSPEEDX = 100;
	[Export] int MAXSPEEDY = 300;
	[Export] int ACCELERATION = 1000;
	[Export] int GRAVITY = 1350;
	[Export] int JUMPSPEED = -202;
	[Export] int ONWALLFALLSPEED = 100;
	[Export] int MAXCLIMBSPEED = 80;
	[Export] int CLIMBACCELERATION = 500;
	[Export] float WALLFRICTIONFACTOR = .01f;
	[Export] float WALLJUMPFACTORX = 1.4f;
	[Export] float WALLJUMPFACTORY = 1.2f;
	[Export] int DEFAULTDASHCOUNT = 1;
	[Export] int DASHSPEED = 310;
	private Vector2 E1 = new Vector2(1, 0);
	private Vector2 E2 = new Vector2(0, 1);

	// Frame Data Constants
	[Export] int MAXJUMPFRAME = 10;
	[Export] int FRAMELOCKXY = 5;
	[Export] int INPUTBUFFERMAX = 5;
	[Export] int DASHFRAMELOCK = 9;
	[Export] int ATTACKFRAMELOCK = 10;
	[Export] int WALLBUFFERMAX = 5;

	// Object Constants
	
	// Player Movement Variables
	private Vector2 velocity = new Vector2(0,0);
	private Vector2 userInput = new Vector2(0,0);
	private bool isDead = false;
	private bool lastOnFloor = false;
	private bool justPressedJump = false;
	private bool isFastFalling = false;
	private bool isDashing = false;
	private bool wasOnWall = false;
	private bool isClimbing = false;
	private float wallFrictionFactor = 1;
	private float jumpStrength = 0;
	private int lastCollisionDirectionX = 1;
	private int lastCollisionDirectionY = 1;
	private int lastFacingDirection = 1;
	private int frameLockX = 0;
	private int frameLockY = 0;
	private int dashLock = 0;
	private int attackLock = 0;
	private int jumpFrames = 0;
	private int jumpBufferFrames = 0;
	private int wallBufferFrames = 0;
	private int dashCount = 0;
	private int attackDirection = 3;
	
	// Animation Variables
	private AnimatedSprite playerAnimation;

	// [TODO] Try animationTree and Player since it provides more functionality. 
	// Try both methods, see what works best
	
	// Called when the node enters the scene tree for the first time.
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
		// Setting up signals
		baseWorld = this.Owner as World;
		if(baseWorld == null)
		{
			throw new ArgumentNullException("Edwin: World is not found");
		}
		Connect("PlayerDeath", baseWorld, "HandlePlayerDeath");

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
		// Checking collisions
		int totalCollisions = GetSlideCount();
		for(int i = 0; i < totalCollisions; i++)
		{
			KinematicCollision2D currentCollision = GetSlideCollision(i);
			Godot.Object collidedWith = currentCollision.Collider;
			if ((collidedWith as TMDanger) != null)
			{
				EmitSignal("PlayerDeath");
				GD.Print("Player has died");

				// this bool begins death animation
				isDead = true;
			}
			if ((collidedWith as TMClimable) != null)
			{
				isClimbing = true;
				GD.Print("Touch a climable surface");
				// TO-DO : Add climbing functionality and movement
				// Climbing functionality will most likely not be part
				// of TileMap, but just an Area2D scene.
				
			} 
			else
			{
				isClimbing = false;
			}
		}

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
		
		// Makes every new position a while number in the form of a float. This
		// is to stop the screen from jittering and shaking randomly. 
		// snaps pixels to nearest pixel to remove pixel jitter
		// another solution is snapping camera to nearest pixel
		// problem with pixels stretching is because we scaled player.
		Position = new Vector2((float)Math.Round(Position.x), (float)Math.Round(Position.y));
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
		userInput.y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down");
		jumpStrength = Input.GetActionStrength("ui_jump");

		// Update last facing direction accordingly
		lastFacingDirection = (userInput.x == 0) ? lastFacingDirection : Math.Sign(userInput.x);
		
		// Dashing is implemented here.
		if (dashLock == 0)
		{
			// Dash condition: just pressed dash button (not held) and we have
			// another dash avaliable. We only check this when frameLockX is
			// frameLockX should prevent any horizontal movement changes. We
			// also set frameLockY and 0 velocity.y so that the player can
			// "float" while dashing.
			if (Input.IsActionJustPressed("ui_dash") && dashCount > 0)
			{
				isDashing = true;
				dashCount--;
				frameLockX = DASHFRAMELOCK;
				frameLockY = DASHFRAMELOCK;
				dashLock   = DASHFRAMELOCK;
				velocity.x = DASHSPEED * lastFacingDirection;
				velocity.y = 0;
				jumpFrames = 0;
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
		else
		{
			dashLock--;
		}

		// This code here is for buffering a jump. 
		if (Input.IsActionJustPressed("ui_jump"))
		{
			jumpBufferFrames = INPUTBUFFERMAX;
			justPressedJump = true;
		}
		else if (jumpBufferFrames > 0)
		{
			jumpBufferFrames--;
		}
		else
		{
			justPressedJump = false;
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

		// count down the buffered wall frames. This is here so that the player
		// may wall jump a few frames after letting go of the wall. Added to be
		// more user friendly.
		if (IsOnWall())
		{
			wallBufferFrames = WALLBUFFERMAX;
			wasOnWall = true;
		}
		else if (wallBufferFrames > 0)
		{
			wallBufferFrames--;
		}
		else
		{
			wasOnWall = false;
		}

		// What we want for attack:
		// + cant attack if just attacked less than ATTACKFRAMELOCK frames ago
		// + when attacking, must remember to add this attack frame lock
		// + cannot attack when on the wall
		// + can dash into an attack : YES
		// + can attack into a dash : YES
		// + can attack into a walljump/regular jump : YES
		// + walljump/jump into an attack : YES
		// + will have to change jump button to "space bar" to enable up attack without jumping. (result in minor changes
		// to HelperUpdatePlayerState)
		// + when the player is standing still and attacking, will attack in direction that they are facing
		// + when the player is in the air and attacks while holding up, attack should be facing up. Same if holding down in air,
		// so left and right have least priority in the air (should probably add a ui_attackLeft, ui_attackRight, ui_attackUp, ui_attackDown
		// for support for game controllers, kinda like the c-stick in smash)
		// - have knockback on the player if attack colides with something (preserve momentum but change direction)
		// ====
		// - will have to work on hitbox stuff to implement knockback (last step)

		// Getting the direction of the attack by taking the difference of the
		// up and down keys strength. If they are both held, or neither is held
		// we resord to lastFacingDirection since up and down direction should 
		// have priority for in-air attacks. since value of lastFacingDirection
		// and attackDireciton are both -1/1 we add 1 to lastFacingDirection
		// and to the final result to get values 0, 1, 2, 3 -> down, left, up, 
		// right.
		attackDirection = (int)Input.GetActionStrength("ui_up") - (int)Input.GetActionStrength("ui_down");
		attackDirection = (attackDirection == 0) ? (lastFacingDirection + 1) : attackDirection;
		attackDirection++;
		if (Input.IsActionJustPressed("ui_attack") && attackLock == 0 && !IsOnWall())
		{
			attackLock = ATTACKFRAMELOCK;
			switch (attackDirection)
			{
				case 0:
					GD.Print("down");
					break;
				case 1:
					GD.Print("left");
					break;
				case 2:
					GD.Print("up");
					break;
				case 3:
					GD.Print("right");
					break;
				default:
					GD.Print("Number theory stopped working");
					break;
			}
		}
		else if (attackLock > 0)
		{
			attackLock--;
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
			// Calculate the wall friction factor so that we may have smooth
			// player movement when falling/sliding/moving on walls. Since 
			// WALLFRICTIONFACTOR is so small, the player is allowed to stay
			// on a wall for a while before begining to slide down. This is an
			// accidental feature, but a good one to have :)
			wallFrictionFactor = (velocity.y > ONWALLFALLSPEED) ? 1.5f : WALLFRICTIONFACTOR;

			// Implementation of climbing controls. Make sure that the player
			// is in a climbing state, is on the wall, and pushing against the
			// wall to be considered climbing.
			if (isClimbing && IsOnWall() && userInput.x == lastCollisionDirectionX)
			{
				// Player is pressed up against the wall and holding up/down
				// begin accelerating in that direction. We apply an extra -
				// sign so that the up direction is negative as it should be.
				if (userInput.y != 0)
				{
					velocity.y = HelperMoveToward(velocity.y, -Math.Sign(userInput.y) * MAXCLIMBSPEED, delta * CLIMBACCELERATION);
				}
				// if the player is ONLY pressed up against the wall dont fall.
				else if (userInput.y == 0)
				{
					velocity.y = 0;
				}
			}
			// We want a "friction" like thing when player is on a wall, but 
			// only if he is already falling not when he is on the way up.
			else if (IsOnWall() && velocity.y > 0)
			{
				velocity.y = HelperMoveToward(velocity.y, ONWALLFALLSPEED, delta * GRAVITY * wallFrictionFactor);
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
					jumpBufferFrames = 0;
				}
				// When player is on wall, use direction of last collision to
				// jump away from wall and update velocity vector. Set frame
				// lock x so that user cant go against own wall jump momentum.
				// Also, lastCollisionDirectionX is a nonzero value. If on a
				// climbable surface we dont change anything manually since
				// exiting the area will result in isClimbing to be updated.
				else if (wallBufferFrames > 0)
				{
					velocity.x = Math.Sign(lastCollisionDirectionX) * -WALLJUMPFACTORX * MAXSPEEDX;
					velocity.y = JUMPSPEED * WALLJUMPFACTORY;
					frameLockX = FRAMELOCKXY;
					jumpFrames = 0;
					lastOnFloor = false;
					jumpBufferFrames = 0;
				}
			}
			// User is holding down the jump button after starting a jump. This
			// is to get variable jump heights. 
			if (jumpStrength != 0 && jumpFrames != 0)
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
	
	// Helper function to do linear-introperlation without having to caluclate
	// it manually and just make it depend on a known working function.
	// 
	// Parameters 
	// ----------
	// current : current value
	// desire  : desired value
	// acceleration : step to move by
	// 
	// Returns
	// -------
	// linearly interpolated value
	// 
	private float HelperMoveToward(float current, float desire, float acceleration)
	{
		return (E1 * current).MoveToward(E1 * desire, acceleration).x;
	}
	
	// Plays the animations for the Player within one function rather than 
	// split among multiple functions
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//
	private void PlayAnimation()
	{
		// determines where player faces
		if (velocity.x < 0)
		{
			playerAnimation.FlipH = true;
		}
		else if (velocity.x > 0)
		{
			playerAnimation.FlipH = false;
		}

		// Running
		if (IsOnFloor())
		{
			if (velocity.x != 0)
			{
				playerAnimation.Play("Run");
			}
			else
			{
				playerAnimation.Play("Idle");
			}
		} 
		// Jump
		else if (!isClimbing)
		{	
			if (velocity.y < 0)
			{
				playerAnimation.Play("Jump0");
			}
			else if (velocity.y > 0)
			{
				if (IsOnWall())
				{
					playerAnimation.Play("WallSlide");
				}
				else
				{
					playerAnimation.Play("Jump1");
				}
			}
		}
		else
		{
			if (velocity.y != 0 && IsOnWall())
				playerAnimation.Play("Climb");
			else if (velocity.y == 0 && IsOnWall())
				playerAnimation.Play("WallSlide");
			else if (velocity.y < 0)
				playerAnimation.Play("Jump0");
			else
				playerAnimation.Play("Jump1");
		}
		
		// Dash
		if (isDashing)
		{
			playerAnimation.Play("Dash");
		}

		// Death
		if (isDead)
		{
			PlayDeathAnimation();
		}
	}
	
	// Instances a player death animation that is independent from the player. 
	// Plays player death animation
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//
	private void PlayDeathAnimation()
	{
		PackedScene PlayerDeath = GD.Load<PackedScene>("res://Player/PlayerDeath.tscn");
		Node2D playerDeathEffect = (Node2D) PlayerDeath.Instance();
		// NOTE: Must add instance as child of world, not player because player
		// will be freed and instance will not be able to access player
		GetParent().AddChild(playerDeathEffect); 
		playerDeathEffect.GlobalPosition = GlobalPosition;
		// put camera here with position GLOBAL POSITION
		QueueFree();
	}
	
	// =============================== Signals ===============================
	// =============================== Signals ===============================
	// =============================== Signals ===============================
	
	// Signal - Makes the player jump some fixed height when colliding with
	// some effect box. Uses jumpBufferFrames to make sure that the player
	// can jumped right when hitting this box and get a bigger jump.
	//
	// Parameters 
	// ----------
	// area : area2d that was collided with
	//
	// Returns
	// -------
	//
	private void OnFixedBounceableAreaEntered(object area)
	{
		// dont need to set jumpBufferFrames = 0 since it is unexpected that
		// the player will touch the ground in 10 frames or less. If we really
		// wanted to do it, we could need another conditional expression or
		// statement to do this here.
		velocity.y = (jumpBufferFrames > 0) ? -500 : -400;
	}
	
	// Signal - since the player only has one HP (health point) then whenever
	// we collide with anything on our hurtbox, we are automatically dead.
	//
	// Parameters 
	// ----------
	// area : area2d that we collided with
	// 
	// Returns
	// -------
	//
	private void OnHurtboxBodyAreaEntered(object area)
	{
		isDead = true;
	}
	
}

