using Godot;
using System;

public class Follow : Camera2D
{
	// Camera Constants
	private float STANDARDSCALE = 1.00f;
	private int STANDARDLIMIT = 10000000;
	// limits here for boundaries
	
	// Camera Variables
	private Vector2 zoom = new Vector2(1, 1);

	// [Consider] [TODO] Make camera parent of world not player

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateZoom(STANDARDSCALE, STANDARDSCALE);
//		SetLimits(-192, 225, 0, 320); // Vertical
		SetLimits(0, 225, 0, 1000); // Horizontal
//		NoFollow(); // Static
		MakeCurrent();
	}
	
	// Update zoom, made public so that can be updated by signals in
	// levels.
	// 
	// Parameters
	// ----------
	// scale_x : scale zoom in x direction
	// scale_y : scale zoom in y direction
	// 
	// Returns
	// -------
	// 
	public void UpdateZoom(float scale_x, float scale_y)
	{
		zoom = new Vector2(scale_x, scale_y);
		SetZoom(zoom);
	}
	
	public void FollowHorizontal()
	{
		// These are variables that are defined in parent class
		LimitTop = 0;
		LimitBottom = 0;
	}
	
	public void FollowVertical()
	{
		// These are variables that are defined in parent class
		LimitLeft = 0;
		LimitRight = 320;
	}
	
	public void NoFollow()
	{
		// These are variables that are defined in parent class
		LimitBottom = 0;
		LimitTop = 0;
		LimitLeft = 0;
		LimitRight = 400; // 320
	}
	
	public void SetLimits(int up, int down, int left, int right)
	{
		// These are variables that are defined in parent class
		LimitTop = up;
		LimitBottom = down;
		LimitLeft = left;
		LimitRight = right;
	}
	
	public void Reset()
	{
		// These are variables that are defined in parent class
		LimitTop = -STANDARDLIMIT;
		LimitBottom = STANDARDLIMIT;
		LimitLeft = -STANDARDLIMIT;
		LimitRight = STANDARDLIMIT;
	}
}
