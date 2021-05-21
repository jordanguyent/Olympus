using Godot;
using System;
using System.Collections.Generic;

public class tempOptionScreen : MarginContainer
{
	[Signal]
	public delegate void StartGame(int SaveFileNum, string Password);
	[Signal]
	public delegate void GoToMainMenu();

	//This is the password we use to encrypt the json storing the game/save data
	private string Password = "yeaaa yeaaa";
	private string DefaultDataPath = "res://Data/DefaultData.json";


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

}
