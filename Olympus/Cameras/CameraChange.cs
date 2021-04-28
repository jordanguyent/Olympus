using Godot;
using System;

public class CameraChange : Area2D
{
	[Signal] 
	public delegate void area_entered();

	[Signal]
	public delegate void change_camera_settings(int state, Vector2 offset, Vector2 tlpl, Vector2 brpl, Vector2 zoom, float smoothingDuration, int ID);

	[Export] int cameraID = 1;
	[Export] int state = 1;
	[Export] bool runOnce = false;
	[Export] Vector2 offset = Vector2.Zero;
	[Export] Vector2 scale = Vector2.One;
	[Export] float smoothingDuration = 0.1f;
	[Export] Vector2 topLeftPosLim = Vector2.Zero;
	[Export] Vector2 botRightPosLim = new Vector2(400, 225);

	private Player player = null;
	private Camera2D playerCam = null;
	private bool hasRan = false;

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
		if (runOnce)
		{
			if (!hasRan)
			{
				hasRan = true;
				EmitSignal("change_camera_settings", state, offset, scale, topLeftPosLim, botRightPosLim, smoothingDuration, cameraID);
			}
		}
		else 
		{
			EmitSignal("change_camera_settings", state, offset, scale, topLeftPosLim, botRightPosLim, smoothingDuration, cameraID);
		}
	}
}
