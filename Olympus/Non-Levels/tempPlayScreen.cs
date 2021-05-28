using Godot;
using System;
using System.IO;
using System.Collections.Generic;

public class tempPlayScreen : MarginContainer
{
	[Signal]
	public delegate void StartGame(int SaveFileNum);
	[Signal]
	public delegate void CreateGameAndStart(int SaveFileNum, string Password);
	[Signal]
	public delegate void DeleteGame(int SaveFileNum, string Password);    
	[Signal]
	public delegate void ReturnToMainMenu();
	[Signal]
	public delegate void RaisePrompt(int SaveFileNum);

	//Bool to check whether to take in input or not
	private bool AcceptInput = true;

	//Default data to load in if we create a new save file
	private string DefaultDataPath = "res://Data/DefaultData.json";

	//Child which is the dialogue prompt when deleting
	DeleteDialogue DELETEDIALOGUE;

	//Paths to the arrows we show
	private List<string> LeftArrowPaths = new List<string>{
		{"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/>"}, //Play Game
		{"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/>"}, //Options
		{"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/>"} //Exit
	};

	private List<string> SaveFileDescPaths = new List<string>{
		{"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/Save 1"},
		{"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/Save 2"},
		{"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/Save 3"},
	};

	private List<string> DeleteArrowPaths = new List<string>{
		{"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/Delete >"},
		{"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/Delete >"},
		{"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/Delete >"}
	};

	private List<string> DeleteOptionPaths = new List<string>{
		{"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/Delete Save"},
		{"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/Delete Save"},
		{"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/Delete Save"}
	};

	//Paths to the text the arrows point to

	//Labels we modify to change text
	private List<Label> LeftArrows = new List<Label>();
	private List<Label> SaveFileDescs = new List<Label>();
	private List<Label> RightArrows = new List<Label>();
	private List<Label> DeleteOptions = new List<Label>();

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
				LeftArrows[_OptionNumber].Text = "";
				RightArrows[_OptionNumber].Text = "";
				_OptionNumber = value;
				LeftArrows[_OptionNumber].Text = ">";
			}
		}
	}

	public override void _Ready()
	{
		//Getting the autoload SceneHandler and connecting signals
		SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
		if(SCNHAND == null){
			throw new ArgumentNullException("Autoload Scenehandler not found");
		}
		DELETEDIALOGUE = (DeleteDialogue)GetNode("DeleteDialogue");
		if(DELETEDIALOGUE == null){
			throw new ArgumentNullException("Delete Dialogue not found");
		}
		Connect("StartGame", SCNHAND, "StartGame");
		Connect("ReturnToMainMenu", SCNHAND, "ReturnToMainMenu");
		Connect("RaisePrompt", DELETEDIALOGUE, "RaisePrompt");
		
		//Getting the labels/save files from the label/save file paths from within the scene
		MaxOptionLength = LeftArrowPaths.Count;
		OptionNumber = 0;

		foreach(string LeftArrowPath in LeftArrowPaths){
			LeftArrows.Add( (Label)GetNode(LeftArrowPath) );
		}
		foreach(string SaveFileDescPath in SaveFileDescPaths){
			SaveFileDescs.Add( (Label)GetNode(SaveFileDescPath) );
		}
		foreach(string DeleteArrowPath in DeleteArrowPaths){
			RightArrows.Add( (Label)GetNode(DeleteArrowPath) );
		}
		foreach(string DeleteOption in DeleteOptionPaths){
			DeleteOptions.Add( (Label)GetNode(DeleteOption) );
		}

		LeftArrows[OptionNumber].Text = ">";
		//Adding text to the save files/delete option - will show empty OR stage info + delete option
		Godot.File SaveFile = new Godot.File();
		for(int i = 0; i < MaxOptionLength; i++){
			int SaveFileNum = i + 1;
			SaveFileDescs[i].Text = $"Save {SaveFileNum}";
			if( ! SaveFile.FileExists($"user://save{SaveFileNum}.json") ){
				SaveFileDescs[i].Text += $" - Empty Save";
			}else{
				string SavePath = $"user://save{SaveFileNum}.json";
				Godot.File LoadSaveData = new Godot.File();

				//Use this in actual deployment
				//LoadSaveData.OpenEncryptedWithPass(SavePath, Godot.File.ModeFlags.Read, SCNHAND.Password);
				//Use this during dev/debugging
				LoadSaveData.Open(SavePath, Godot.File.ModeFlags.Read);
				var SaveFileDict = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(LoadSaveData.GetLine()).Result );
				LoadSaveData.Close();

				Godot.Collections.Array StageData = (Godot.Collections.Array)SaveFileDict["WorldData"];
				SaveFileDescs[i].Text += $" - Stage {StageData[0]}-{StageData[1]}";
				DeleteOptions[i].Text = $"Delete Save";
			}
		}
	}

	public override void _Process(float delta)
	{
		if(AcceptInput){
			//Move arrow in the UI/menu
			if(Input.IsActionJustPressed("ui_up")){
				OptionNumber --;
			}
			if(Input.IsActionJustPressed("ui_down")){
				OptionNumber ++;
			}
			if(Input.IsActionJustPressed("ui_right")){
				if(DeleteOptions[OptionNumber].Text == $"Delete Save"){
					LeftArrows[OptionNumber].Text = "";
					RightArrows[OptionNumber].Text = ">";
				}
			}
			if(Input.IsActionJustPressed("ui_left")){
				if(DeleteOptions[OptionNumber].Text == $"Delete Save"){
					RightArrows[OptionNumber].Text = "";
					LeftArrows[OptionNumber].Text = ">";
				}
			}
			//Actions of creating game, starting game, deleting game, back to main menu 
			if(Input.IsActionJustPressed("ui_accept")){
				GD.Print("ui_accept");
				//Left arrow
				if(LeftArrows[OptionNumber].Text == ">"){
					GD.Print("left");
					int SaveFileNum = OptionNumber + 1;
					Godot.File SaveFile = new Godot.File();
					//Create save if it doesn't exist
					if( ! SaveFile.FileExists($"user://save{SaveFileNum}.json") ){
						CreateSave(SaveFileNum);
					}
					EmitSignal("StartGame", SaveFileNum);
				}else
				//Right arrow
				if(RightArrows[OptionNumber].Text == ">"){
					GD.Print("right");
					// AcceptInput = false;
					// EmitSignal("CreatePopup", OptionNumber + 1);
					int SaveFileNum = OptionNumber + 1;

					EmitSignal("RaisePrompt", SaveFileNum);
				}
			}
			if(Input.IsActionJustPressed("ui_cancel")){
				EmitSignal("ReturnToMainMenu");
			}
		}
	}

	public void CreateSave(int SaveFileNum){
		string SavePath = $"user://save{SaveFileNum}.json";

		//Creates a save and loads a new game with default data
		Godot.File LoadDefaultData = new Godot.File();
		LoadDefaultData.Open(DefaultDataPath, Godot.File.ModeFlags.Read);
		var DefaultList = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(LoadDefaultData.GetLine()).Result );
		LoadDefaultData.Close();

		//SaveFile.OpenEncryptedWithPass(SavePath, Godot.File.ModeFlags.Write, Password);
		Godot.File SaveFile = new Godot.File();
		SaveFile.Open(SavePath, Godot.File.ModeFlags.Write);
		SaveFile.StoreString(JSON.Print(DefaultList));
		SaveFile.Close();

		EmitSignal("StartGame", SaveFileNum);
	}

	public void DeleteSave(int SaveFileNum){
		string SavePath = $"user://save{SaveFileNum}.json";
		Godot.File GodotFile = new Godot.File();
		GodotFile.Open(SavePath, Godot.File.ModeFlags.Read);
		string AbsolutePath = GodotFile.GetPathAbsolute();
		GodotFile.Close();

		System.IO.File.Delete(AbsolutePath);
	}

	public void GetConfirmation(bool Action){
		DELETEDIALOGUE.On = false;
		DELETEDIALOGUE.InnerOn = false;
		//If user chose yes
		if(Action){
			int SaveFileNum = OptionNumber + 1;
			DeleteSave(SaveFileNum);

			LeftArrows[OptionNumber].Text = ">";
			SaveFileDescs[OptionNumber].Text = $"Save {SaveFileNum} - Empty Save";
			RightArrows[OptionNumber].Text = "";
			DeleteOptions[OptionNumber].Text = "";
		}
		//If user chose no, don't do anything
	}
}
