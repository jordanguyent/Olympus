using Godot;
using System;

public class Deaths : Label
{
	PlayerData PLAYERDATA = null;
	public override void _Ready()
	{
		PLAYERDATA = GetNode<PlayerData>("/root/PlayerData");
	}

	public override void _Process(float delta)
	{
		Text = "Deaths: " + PLAYERDATA.GetDeathCount().ToString();
	}
}
