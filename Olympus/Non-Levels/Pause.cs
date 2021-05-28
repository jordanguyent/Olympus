using Godot;
using System;
using System.Collections.Generic;

public class Pause : CanvasLayer
{
	[Signal]
	public delegate void Exit();

	public bool On = false;

	MarginContainer PauseLayer;

	private List<string> Select = new List<string>{
		{"MarginContainer/Select/VBoxContainer/Unpause/HBoxContainer/>"},
		{"MarginContainer/Select/VBoxContainer/Game Options/HBoxContainer/>"},
		{"MarginContainer/Select/VBoxContainer/Exit/HBoxContainer/>"}
	};

	//Paths in the scene to the save file labels
	private List<string> PauseMenuOptions = new List<string>{
		{"MarginContainer/Select/VBoxContainer/Unpause/HBoxContainer/Unpause"},
		{"MarginContainer/Select/VBoxContainer/Game Options/HBoxContainer/Game Options"},
		{"MarginContainer/Select/VBoxContainer/Exit/HBoxContainer/Exit"}
	};
	//The actual text to put at the pause options
	private List<string> PauseMenuOptionText = new List<string>{
		{"Unpause"},
		{"Game Options"},
		{"Exit"}
	};
	//Contains the labels we modify to change text
	private List<Label> SelectList = new List<Label>();
	private List<Label> PauseOptionList = new List<Label>();

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
				SelectList[_OptionNumber].Text = "";
				_OptionNumber = value;
				SelectList[_OptionNumber].Text = ">";
			}
		}
	}

	public override void _Ready()
	{
		//Getting the autoload scene handler and connecting signals
		SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
		if(SCNHAND == null){
			throw new ArgumentNullException("Edwin: Autoload SceneHandler not found"); 
		}
		Connect("Exit", SCNHAND, "ReturnToMainMenu");
		//Getting the labels/save files from the label/save file paths from within the scene
		MaxOptionLength = Select.Count;
		foreach(string Selector in Select){
			SelectList.Add(GetNode(Selector) as Label);
		}
		for(int i = 0; i < PauseMenuOptions.Count; i++){
			PauseOptionList.Add(GetNode(PauseMenuOptions[i]) as Label);
			PauseOptionList[i].Text = PauseMenuOptionText[i];
		}
		SelectList[OptionNumber].Text = ">";
		//Setting background
		PauseLayer = (MarginContainer)GetNode("MarginContainer");
		PauseLayer.Visible = !PauseLayer.Visible;
	}

	public override void _Process(float delta)
	{
		if(this.On){
			if(Input.IsActionJustPressed("ui_cancel")){
				OptionNumber = 0;
				PauseLayer.Visible = !PauseLayer.Visible;
				GetTree().Paused = !GetTree().Paused;
			}
			if(Input.IsActionJustPressed("ui_up") && GetTree().Paused){
				OptionNumber --;
			}
			if(Input.IsActionJustPressed("ui_down") && GetTree().Paused){
				OptionNumber ++;
			}
			if(Input.IsActionJustPressed("ui_accept") && GetTree().Paused){
				if(OptionNumber == 0){
					PauseLayer.Visible = !PauseLayer.Visible;
					GetTree().Paused = !GetTree().Paused;
				}
				if(OptionNumber == 2){
					PauseLayer.Visible = !PauseLayer.Visible;
					GetTree().Paused = !GetTree().Paused;
					this.On = false;
					EmitSignal("Exit");
				}
			}
		}
	}
}
