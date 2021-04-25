using Godot;
using System;

public class Follow : Camera2D
{

	// Camera Constants
	private int STANDARDLIMIT = 10000000;
	private int CAMERAWIDTH;
	private int CAMERAHEIGHT;
	
	// Camera Variables
	private float smoothingDuration = .1f;
	private float updatedSmoothDuration;
	private Vector2 currentPosition;
	private Vector2 destinationPosition;
	private Vector2 offset = Vector2.Zero;
	private Vector2 targetPos;
	private Vector2 clampedPos;
	private bool cameraClamped = false;
	private int lPosLim = -10000000;
	private int rPosLim = 10000000;
	private int uPosLim = -10000000;
	private int dPosLim = 10000000;

	private int currentCameraID = 1;
	private int newCameraID = 1;
	
	// initialize timer
	private int initTimer = 1;


	// Parent Node
	private Node2D parent = null;

	enum CameraState 
	{
		Init,
		Static,
		Dynamic,
		Transition
	}

	// Camera State
	CameraState state = CameraState.Init;
	CameraState changeState;

	// [Consider] [TODO] Make camera parent of world not player

	// NOTE: Set the LIMIT to the size of the stage.
	// Set the POSITION to place the camera is designated area within the limit.
	// Need function to change position. 

	// Create these functions, these functions most likely going to be running constantly
	// FollowPlayer
	// LimitPosition
	// PositionOffsetFromPlayer
	// SetPosition
	// DragMargin // may need a grid position
	// may need to enable smoothing in some cases
	//	When camera follows player, no smoothing, when moving
	//	camera to specific position, smoothing on.
	// NVM, must have camera smoothing on, but on place, smoothing speed is 
	// large


	// anchoring
	// changing pos limit
	// player follow
	// player enters area, emit signal to camera to change camera settings

	// States:
	// locked on player
	// static
	// 

	

	// make simple fade in and out transition. can be used for everything

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// initialization
		SetAsToplevel(true);

		// get node
		parent = GetNode<Node2D>("../");

		// initialize constants
		CAMERAWIDTH = (int) GetViewport().GetVisibleRect().Size.x;
		CAMERAHEIGHT = (int) GetViewport().GetVisibleRect().Size.y;

		// initialize variables
		currentPosition = parent.Position;
		targetPos = parent.Position;

		// Set camera to current
		MakeCurrent();
	}

	// small stutter when changing camera settings.
	// may need to fix transition state for now.

	public override void _PhysicsProcess(float delta)
	{
		switch (state)
		{
			case CameraState.Init:
				currentPosition = parent.Position;
				state = CameraState.Dynamic;
				break;

			case CameraState.Static:
				clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
				ArriveTo(clampedPos, Vector2.Zero, delta);
				break;

			case CameraState.Dynamic:
				clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
				targetPos = (cameraClamped) ? clampedPos : parent.Position;
				ArriveTo(targetPos, offset, delta);
				break;

			case CameraState.Transition:
				if (currentCameraID != newCameraID)
				{
					smoothingDuration = 0.2f;
					clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
					targetPos = (cameraClamped) ? clampedPos : parent.Position;
					ArriveTo(targetPos, offset, delta);
					GD.Print(Position.Snapped(Vector2.One), " ", targetPos.Snapped(Vector2.One));
					if (Position.Snapped(Vector2.One) == targetPos.Snapped(Vector2.One))
					{
						smoothingDuration = updatedSmoothDuration;
						currentCameraID = newCameraID;
						state = changeState;
					}
				}
				else
				{
					smoothingDuration = updatedSmoothDuration;
					state = changeState;
				}
				
				break;
		}
	}
	
	public void SetLimits(int up, int down, int left, int right)
	{
		// These are variables that are defined in parent class
		LimitTop = up;
		LimitBottom = down;
		LimitLeft = left;
		LimitRight = right;
	}

	public Vector2 ArriveTo(Vector2 targetPosition, Vector2 offset, float delta)
	{
		destinationPosition = targetPosition + offset; // offset goes here
		Vector2 distance = new Vector2(destinationPosition.x - currentPosition.x, destinationPosition.y - currentPosition.y);
		if (Math.Abs(distance.x) < .3)
		{
			distance.x = 0; 
		}
		if (Math.Abs(distance.y) < .3)
		{
			distance.y = 0;
		}
		currentPosition +=  distance / smoothingDuration * delta;
		Position = currentPosition;
		
		// // Snaps camera to pixel after smoothing is finished
		// if (distance / smoothingDuration * delta == Vector2.Zero)
		// {
		// 	Position = Position.Snapped(Vector2.One);
		// }

		ForceUpdateScroll();

		return distance;
	}

	public Vector2 ClampPosition(int x1, int x2, int y1, int y2)
	{
		float x;
		float y;

		int camOffsetWidth = 0;
		int camOffsetHeight = 0;
		// int camOffsetWidth = CAMERAWIDTH / 2;
		// int camOffsetHeight = CAMERAHEIGHT / 2;
		
		Position = parent.Position;

		if (Position.x < x1 + camOffsetWidth)
		{
			cameraClamped = true;
			x = x1 + camOffsetWidth;
		} 
		else if (Position.x > x2 - camOffsetWidth)
		{
			cameraClamped = true;
			x = x2 - camOffsetWidth;
		}
		else 
		{
			cameraClamped = false;
			x = Position.x;
		}

		if (Position.y < y1 + camOffsetHeight)
		{
			cameraClamped = true;
			y = y1 + camOffsetHeight;
		}
		else if (Position.y > y2 - camOffsetHeight)
		{
			cameraClamped = true;
			y = y2 - camOffsetHeight;
		}
		else 
		{
			cameraClamped = false;
			y = Position.y;
		}

		return new Vector2(x, y);
	}

	public void ChangeCameraSettings(int st, Vector2 off, Vector2 scale, Vector2 tlPosLim, Vector2 brPosLim, float sd, int id)
	{
		// changes the state
		switch (st)
		{
			case 1:
				changeState = CameraState.Init;
				break;

			case 2:
				changeState = CameraState.Static;
				break;

			case 3:
				changeState = CameraState.Dynamic;
				break;
			default:
				throw new ArgumentOutOfRangeException("Error: int state = " + st.ToString() + " is out of range");
		}

		state = CameraState.Transition;
		newCameraID = id;

		offset = off;

		updatedSmoothDuration = sd;
		Zoom = scale;

		lPosLim = (int) tlPosLim.x;
		uPosLim = (int) tlPosLim.y;
		rPosLim = (int) brPosLim.x;
		dPosLim = (int) brPosLim.y;
	}
}
