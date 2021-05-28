using Godot;
using System;

public class Follow : Camera2D
{
	// loads camera frames after player
	[Export] int loadDelay = 1;

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
	private Vector2 clampedPos;
	private bool cameraClamped = false;
	private int lPosLim = -10000000;
	private int rPosLim = 10000000;
	private int uPosLim = -10000000;
	private int dPosLim = 10000000;
	private int currentCameraID = 1;
	private int newCameraID = 1;
	private bool cameraInit = false;

	// Parent Node
	private Node2D parent = null;

	// Temp solution to camera load
	int loadTimer = 1;

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

	// Called when node first enters the scene.
	// Sets the camera to the top level and establishes parent and camera width
	// and height
	//
	// Parameters 
	// ----------
	// 
	// Returns
	// -------
	// 
	public override void _Ready()
	{
		SetAsToplevel(true);

		// get node
		parent = GetNode<Node2D>("../");
		
		// initialize constants
		CAMERAWIDTH = (int) GetViewport().GetVisibleRect().Size.x;
		CAMERAHEIGHT = (int) GetViewport().GetVisibleRect().Size.y;

		// Set camera to current
		MakeCurrent();
	}

	// Called every frame. Camera state is processed in this function.
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
		if (loadDelay <= 0)
		{
			switch (state)
			{
				case CameraState.Init:
					cameraInit = true;
					Position = parent.Position + offset;
					Position = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
					currentPosition = Position;
					state = CameraState.Dynamic;
					break;

				// may need to just remove camera state
				case CameraState.Static:
					clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
					ArriveTo(clampedPos, delta);
					break;

				case CameraState.Dynamic:
					Position = parent.Position + offset; // offset adjusted here
					clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
					ArriveTo(clampedPos, delta);
					break;

				case CameraState.Transition:

					if (!cameraInit && currentCameraID == newCameraID) // snaps camera to target immediately
					{
						smoothingDuration = updatedSmoothDuration;
						state = changeState;
					}
					else // smooths camera to target
					{
						smoothingDuration = 0.2f;
						Position = parent.Position + offset;
						clampedPos = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);
						ArriveTo(clampedPos, delta);
						if (Position.Snapped(Vector2.One) == clampedPos.Snapped(Vector2.One))
						{
							smoothingDuration = updatedSmoothDuration;
							currentCameraID = newCameraID;
							state = changeState;
						}
					}
					
					
					break;
			}
		}
		else
		{
			// Load delay necessary to make sure camera loads at correct position
			loadDelay--;

			// initialize camera position
			Position = parent.Position + offset;
			Position = ClampPosition(lPosLim, rPosLim, uPosLim, dPosLim);

			// initialize variables
			currentPosition = Position;
		}
		
	}

	// Creates camera smoothings from targetPosition to destination.
	//
	// Parameters 
	// ----------
	// targetPosition: camera's destinaton target
	// delta : time elapsed since previous frame
	//
	// Returns
	// -------
	// 
	public void ArriveTo(Vector2 targetPosition, float delta)
	{
		destinationPosition = targetPosition;
		Vector2 distance = new Vector2(destinationPosition.x - currentPosition.x, destinationPosition.y - currentPosition.y);
		currentPosition += distance / smoothingDuration * delta;
		Position = currentPosition.Snapped(Vector2.One);

		ForceUpdateScroll();
	}


	// Creates camera smoothings from targetPosition to destination.
	//
	// Parameters 
	// ----------
	// x1: left bound
	// x2: right bound
	// y1: up bound
	// y2: down bound
	// 
	// Returns
	// -------
	// Vector2: new clamped position vector
	//
	public Vector2 ClampPosition(int x1, int x2, int y1, int y2)
	{
		float x;
		float y;

		int cameraPaddingX = CAMERAWIDTH / 2;
		int cameraPaddingY = CAMERAHEIGHT / 2;

		if (Position.x - cameraPaddingX < x1)
		{
			x = x1 + cameraPaddingX;
		} 
		else if (Position.x + cameraPaddingX > x2)
		{
			x = x2 - cameraPaddingX;
		}
		else 
		{
			x = Position.x;
		}

		if (Position.y - cameraPaddingY - 1 < y1)
		{
			y = y1 + cameraPaddingY + 1;
		}
		else if (Position.y + cameraPaddingY > y2)
		{
			y = y2 - cameraPaddingY;
		}
		else 
		{
			y = Position.y;
		}

		return new Vector2(x, y);
	}

	// SIGNAL
	// Runs when player enters area2D for camera setting change.
	//
	// Parameters 
	// ----------
	// st: new state
	// off: camera offset from target
	// scale: camera zoom
	// tlPosLim: top left bound for position
	// brPosLim: bottom right bound for position
	// sd: new smoothing duration
	// id: camera ID
	// 
	// Returns
	// -------
	//
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
