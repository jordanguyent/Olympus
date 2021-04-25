using Godot;
using System;

public class CameraChange : Area2D
{
	[Signal] 
	public delegate void area_entered();

	[Signal]
	public delegate void change_camera_settings(int state, Vector2 offset, Vector2 tlpl, Vector2 brpl, Vector2 zoom, float smoothingDuration, int ID);

	[Export] float smoothingDuration = 0.1f;
	[Export] Vector2 targetPos = Vector2.Zero;
	[Export] Vector2 offset = Vector2.Zero;
	[Export] int state = 3;
	[Export] Vector2 scale = Vector2.One;
	[Export] Vector2 topLeftPosLim = Vector2.Zero;
	[Export] Vector2 botRightPosLim = new Vector2(225, 400);
	[Export] int cameraID = 1;
	

	/*
	private float smoothingDuration = .1f;
	
	private Vector2 currentPosition;
	private Vector2 destinationPosition;
	private Vector2 offset = Vector2.Zero;
	private Vector2 targetPos;
	*/

	// All exported camera settings


	private Player player = null;
	private Camera2D playerCam = null;

	public override void _Ready()
	{
		player = GetNode<Player>("../Player");    
		playerCam = player.GetNode<Camera2D>("Follow");

		// connect signals
		Connect("area_entered", this, "OnAreaEntered");
		Connect("change_camera_settings", playerCam, "ChangeCameraSettings");
		
	}

	public void OnAreaEntered(object area)
	{
		// fix position limiter
		// pass in all the camera settings
		EmitSignal("change_camera_settings", state, offset, scale, topLeftPosLim, botRightPosLim, smoothingDuration, cameraID);
	}
}
