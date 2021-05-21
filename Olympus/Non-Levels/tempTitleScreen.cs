using Godot;
using System;
using System.Collections.Generic;

public class tempTitleScreen : MarginContainer
{
	[Signal]
	public delegate void MoveToPlayGame();
	[Signal]
	public delegate void MoveToGameOptions();
	[Signal]
	public delegate void MoveToKeybinds();

	//Paths to the labels we modify
	private List<string> LabelPaths = new List<string>{
		{"CenterContainer/VBoxContainer/Select/VBoxContainer/Play/HBoxContainer/>"}, //Play Game
		{"CenterContainer/VBoxContainer/Select/VBoxContainer/Options/HBoxContainer/>"}, //Options
		{"CenterContainer/VBoxContainer/Select/VBoxContainer/Keybinds/HBoxContainer/>"}, //Keybinds
		{"CenterContainer/VBoxContainer/Select/VBoxContainer/Exit/HBoxContainer/>"} //Exit
	};

	//Labels we modify to change text
	private List<Label> LabelList = new List<Label>();

	private int MaxOptionLength;
	private int _OptionNumber;
	public int OptionNumber{
		get{
			return _OptionNumber;
		}
		set{
			if( (_OptionNumber == 0 && value <= _OptionNumber) || (_OptionNumber == (MaxOptionLength - 1) && value > _OptionNumber) ){
				//do nothing
			}else{
				LabelList[_OptionNumber].Text = "";
				_OptionNumber = value;
				LabelList[_OptionNumber].Text = ">";
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Setting up Signals
		SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
		if(SCNHAND == null){
			throw new ArgumentNullException("Edwin: Autoload SceneHandler not found"); 
		}
		Connect("MoveToPlayGame", SCNHAND, "MoveToPlayGame");
		Connect("MoveToGameOptions", SCNHAND, "MoveToGameOptions");
		Connect("MoveToKeybinds", SCNHAND, "MoveToKeybinds");

		//Setting up the selector for the menu
		MaxOptionLength = LabelPaths.Count;
		OptionNumber = 0;
		foreach(string LabelPath in LabelPaths){
			LabelList.Add(GetNode(LabelPath) as Label);
		}
		LabelList[OptionNumber].Text = ">";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if(Input.IsActionJustPressed("ui_up")){
			OptionNumber --;
		}
		if(Input.IsActionJustPressed("ui_down")){
			OptionNumber ++;
		}
		if(Input.IsActionJustPressed("ui_accept")){
			switch(OptionNumber){
				case 0:
					PlayGame();
					break;
				case 1:
					Options();
					break;
				case 2:
					Keybinds();
					break;
				case 3:
					Exit();
					break;
				default:
					break;
			}
		}
	}

	private void PlayGame(){
		EmitSignal("MoveToPlayGame");
	}

	private void Options(){
		EmitSignal("MoveToGameOptions");
	}

	private void Keybinds(){

	}

	private void Exit(){
		GetTree().Quit();
	}
}
