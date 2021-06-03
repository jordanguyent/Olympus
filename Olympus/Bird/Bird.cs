using Godot;
using System;

public class Bird : KinematicBody2D
{


	// state
	enum BirdState {
		Idle,
		Ready,
		Fly,
		Fling,
		Glide,
		Respawn,
		Despawn,
		Death
	};

	// Bird's children nodes
	private AnimatedSprite birdAnimation = null;
	private CollisionShape2D body = null;
	private CollisionShape2D platform = null;

	// Bird variables
	[Export] int ACCELERATION = 1000;
	[Export] int MAXOFFSETX = 0;
	[Export] int MAXOFFSETY = -20;
	[Export] int MAXVELOCITY = 300;
	[Export] int RECALLDELAY = 180;
	[Export] int FLYDELAY = 13;
	private BirdState state = BirdState.Ready;
	private Vector2 userInput = new Vector2();
	private Vector2 velocity = new Vector2();
	private Vector2 destPos = new Vector2();
	private float time = 0.15f; // 0.15
	private int recallTimer;
	private int flyTimer;	

	// Player variables
	private Player player = null;

	// NOTES:
	// colliding with objects changes state. Dependent on current State.
	// collision signal will probably have state machine
	// Either have bird be a created object, have him invisible, or have him follow, or make him ignore obstacles until sent out.
	// May need to the public changeState function for other objects
	// change collision size of thing

	// Called when the node enters the scene tree for the first time.
	// Called when the node enters the scene tree for the first time. Used to
	// connect signals from Bird and initialize objects
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		// Get nodes
		birdAnimation = GetNode<AnimatedSprite>("AnimatedSprite");
		body = GetNode<CollisionShape2D>("Body");
		platform = GetNode<CollisionShape2D>("Platform");
		player = GetNode<Player>("../Player");

		// Set position of bird to player
		Position = player.Position + new Vector2(MAXOFFSETX, MAXOFFSETY);

		// Error checking
		if (player == null) 
			throw new ArgumentNullException("Bird.cs: Player not found!"); 

		// initialize variables
		recallTimer = RECALLDELAY;
		flyTimer = FLYDELAY;
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
		
		// State machine
		// ======================
		// Ready: Fly
		// Fly: Idle
		// Idle: Despawn
		// Despawn: Respawn
		// Respawn: Ready
		// ======================

		// Transitions
		switch (state) 
		{
			case BirdState.Ready:
				// check player direction
				// check input
				// up and down take precedence before left and right
				userInput.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
				userInput.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
				
				if (Input.IsActionJustPressed("ui_send_bird"))
				{
					state = BirdState.Fly;
				}
				break;

			case BirdState.Idle:
				recallTimer--;
				if (recallTimer <= 0) 
				{
					state = BirdState.Despawn;
					recallTimer = RECALLDELAY;
				}
				break;
			
			case BirdState.Fly:
				flyTimer--;
				if (flyTimer <= 0) 
				{
					state = BirdState.Idle;
					flyTimer = FLYDELAY;
				}
				break;

			case BirdState.Fling:
				break;

			case BirdState.Glide:
				break;

			case BirdState.Respawn:
				if (birdAnimation.Frame > 3)
				{
					state = BirdState.Ready;
				}
				break;

			case BirdState.Despawn:
				if (birdAnimation.Frame > 3)
				{
					state = BirdState.Respawn;
				}
				break;

			default:
				throw new ArgumentNullException("Bird.cs: Not a valid BirdState"); 
		}

		// Actions
		switch (state) 
		{
			case BirdState.Ready:
				if (Position != player.Position + new Vector2(MAXOFFSETX, MAXOFFSETY))
				{
					destPos = new Vector2(player.Position.x + MAXOFFSETX, player.Position.y + MAXOFFSETY);
					ArriveTo(delta, destPos);
					AvoidCollisions();
				}
				break;

			case BirdState.Idle:
				SetCollisionLayerBit(6, true);
				body.Disabled = true;
				platform.Disabled = false;
				break;
			
			case BirdState.Fly:
				// determines direction bird flies in
				velocity = new Vector2(userInput.x, userInput.y).Normalized() * MAXVELOCITY;
				velocity = MoveAndSlide(velocity);
				Position = Position.Snapped(Vector2.One);
				break;

			case BirdState.Fling:
				break;

			case BirdState.Glide:
				break;

			case BirdState.Respawn:
				Position = player.Position;
				break;

			case BirdState.Despawn:
				SetCollisionLayerBit(6, false);
				body.Disabled = false;
				platform.Disabled = true;
				break;

			default:
				throw new ArgumentNullException("Bird.cs: Not a valid BirdState"); 
		}

		// Animations
		switch (state) 
		{
			case BirdState.Ready:
				if (Math.Abs(Position.x - destPos.x) <= 10)
					birdAnimation.Play("IdleFly");
				else
					birdAnimation.Play("Fly");

				if (velocity.x < 0) 
					birdAnimation.FlipH = true;
				else if (velocity.x > 0)
					birdAnimation.FlipH = false; 
				break;

			case BirdState.Idle:
				birdAnimation.Play("IdleFly");
				break;
			
			case BirdState.Fly:
				birdAnimation.Play("Fly");
				break;

			case BirdState.Fling:
				birdAnimation.Play("Fling");
				break;

			case BirdState.Glide:
				birdAnimation.Play("Glide");
				break;

			case BirdState.Respawn:
				birdAnimation.Play("Respawn");
				break;

			case BirdState.Despawn:
				birdAnimation.Play("Despawn");
				break;

			default:
				throw new ArgumentNullException("Bird.cs: Not a valid BirdState"); 
		}
	}

	// Moves the bird to a specified location. Ignores collisions. Function
	// may not be needed.
	//
	// Parameters 
	// ----------
	// delta: link with frames
	// targetPosition: location of destination
	//
	// Returns
	// -------
	//   
	private void MoveTo(float delta, Vector2 targetPosition)
	{
		Vector2 distance = new Vector2(targetPosition.x - Position.x, targetPosition.y - Position.y);
		Position += distance / time * delta;
		Position = Position.Snapped(Vector2.One);
	}

	// Moves the bird to a specified location.
	//
	// Parameters 
	// ----------
	// delta: link with frames
	// targetPosition: location of destination
	//
	// Returns
	// -------
	//   
	private void ArriveTo(float delta, Vector2 targetPosition)
	{
		
		Vector2 distance = new Vector2(targetPosition.x - Position.x, targetPosition.y - Position.y);
		velocity = distance / time;
		velocity = MoveAndSlide(velocity);
		Position = Position.Snapped(Vector2.One);
	}

	// Prevents the bird from getting stuck behind obstacles
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	// bool: true if bird collided with world object  
	//
	private void AvoidCollisions()
	{
		int slideCount = GetSlideCount();
		for (int i = 0; i < slideCount; i++) 
		{
			var collision = GetSlideCollision(i);
			Vector2 dir = (collision.Position - Position).Normalized();
			Vector2 playerDir = (player.Position - Position).Normalized();
			if (dir.x > 0 || dir.x < 0)
			{
				if (playerDir.y > 0)
					Position += Vector2.Down;
				else 
					Position += Vector2.Up;
			}
			if (dir.y > 0 || dir.y < 0)
			{
				if (playerDir.x > 0)
					Position += Vector2.Right;
				else 
					Position += Vector2.Left;
			}	
		}
	}	

	// Signal:
	// Connected to Whistle.cs.
	// Bird is recalled to player.
	//
	// Parameters 
	// ----------
	//
	// Returns
	// -------
	//
	private void RecallBird()
	{
		if (state == BirdState.Idle || state == BirdState.Fly) 
		{
			SetCollisionLayerBit(6, false);
			body.Disabled = false;
			platform.Disabled = true;
			recallTimer = RECALLDELAY;
			flyTimer = FLYDELAY;
			birdAnimation.Frame = 0;
			state = BirdState.Despawn;
		}
		
	}
}
