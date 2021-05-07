using Godot;
using System;

public class PlayerData : Node
{
	[Signal] public delegate void timeout();
	Timer time = new Timer();


	static int deathCount = 0;
	int gameTime = 0;

	public override void _Ready()
	{
		time.Connect("timeout", this, "_on_Timer_timeout");
		AddChild(time);
		time.WaitTime = 1;
		time.Start();
	}	

	public void IncrementDeathCount()
	{
		deathCount++;
	}

	public int GetDeathCount()
	{
		return deathCount;
	}

	public int GetTime()
	{
		return gameTime;
	}

	private void _on_Timer_timeout()
	{
		gameTime++;
		time.Start();
	}
}
