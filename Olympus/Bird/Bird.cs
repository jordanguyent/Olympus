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
		Death
	};

	// Bird's children nodes
	private AnimatedSprite birdAnimation = null;

	// Bird variables
	private BirdState state = BirdState.Ready;
	private Vector2 velocity = new Vector2();
	private Vector2 destPos = new Vector2();
	private float time = 0.15f; // 0.15
	[Export] int ACCELERATION = 1000;
	[Export] int MAXOFFSETX = 0;
	[Export] int MAXOFFSETY = 20;

	// Player variables
	private Player player = null;

	// NOTES:
	// colliding with objects changes state. Dependent on current State.
	// collision signal will probably have state machine

	// Either have bird be a created object, have him invisible, or have him follow, or make him ignore obstacles until sent out.

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
		birdAnimation = GetNode<AnimatedSprite>("AnimatedSprite");
		player = GetNode<Player>("../Player");

		Position = player.Position + new Vector2(-MAXOFFSETX, -MAXOFFSETY);

		// Error checking
		if (player == null) 
		{
			throw new ArgumentNullException("Bird.cs: Player not found"); 
		}
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

		// Transitions
		switch (state) 
		{
			case BirdState.Ready:
				break;

			case BirdState.Idle:
				break;
			
			case BirdState.Fly:
				break;

			case BirdState.Fling:
				break;

			case BirdState.Glide:
				break;

			default:
				throw new ArgumentNullException("Bird.cs: Not a valid BirdState"); 
		}

		// Actions
		switch (state) 
		{
			case BirdState.Ready:
				if (Position != player.Position + new Vector2(-MAXOFFSETX, -MAXOFFSETY))
				{
					destPos = new Vector2(player.Position.x - MAXOFFSETX, player.Position.y - MAXOFFSETY);
					// MoveTo(delta, destPos);
					ArriveTo(delta, destPos);
					AvoidCollisions();
				}
				
				break;

			case BirdState.Idle:
				break;
			
			case BirdState.Fly:
				break;

			case BirdState.Fling:
				break;

			case BirdState.Glide:
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

			default:
				throw new ArgumentNullException("Bird.cs: Not a valid BirdState"); 
		}
	}

	// Moves the bird to a specified location. Ignores collisions.
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

	

	
}
