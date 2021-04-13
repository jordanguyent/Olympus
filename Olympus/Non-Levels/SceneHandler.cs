using Godot;
using System;
using System.Collections.Generic;

public class SceneHandler : Node
{
	Godot.Collections.Dictionary<string, string> stageList;
	public Node CurrentScene{get; set;}
	private string CurrentPath;

	private int CurrentLevel;
	private int CurrentStage;

	public Godot.Collections.Array PlayerSpawnPosition = null;

	private string SavePath;

	public void SceneHelper(string Path){
		CurrentScene = GetTree().Root.GetChild(GetTree().Root.GetChildCount() - 1);
		CurrentScene.QueueFree();
		PackedScene CurrentLevel = (PackedScene)GD.Load(Path);
		CurrentScene = CurrentLevel.Instance();
		GetTree().Root.AddChild(CurrentScene);
		GetTree().SetCurrentScene(CurrentScene);
	}

	public void ChangeStage(string ZoneEntered){
		//Get which stage to load next
		Godot.File ZoneFile = new Godot.File();
		string ZonePath = $"res://Levels/Level{this.CurrentLevel}/Level{this.CurrentLevel}Zone.json";
		ZoneFile.Open(ZonePath, Godot.File.ModeFlags.Read);
		var ZoneData = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(ZoneFile.GetLine()).Result );
		ZoneFile.Close();
		//Get where to load in that stage
		Godot.File SpawnFile = new Godot.File();
		string SpawnPath = $"res://Levels/Level{this.CurrentLevel}/Level{this.CurrentLevel}Spawn.json";
		SpawnFile.Open(SpawnPath, Godot.File.ModeFlags.Read);
		var SpawnData = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(SpawnFile.GetLine()).Result );
		SpawnFile.Close();

		var StageData = (Godot.Collections.Dictionary)ZoneData[$"{CurrentStage}"];
		string NextLevel = (string)StageData[ZoneEntered];
		//Use the term "NextWorld" in the json to indicate moving from level 1 to level 2 as an example
		//NOTE: LEVELS =/= STAGES
		if(NextLevel == "NextWorld"){

		}else{
			//Stores the variables used for checking spawn point
			int FromLevel = CurrentStage;
			int ToLevel;
			//Loads new scene
			string CurrentStageScenePath = $"res://Levels/Level{this.CurrentLevel}/Stages/Stage{NextLevel}.tscn";
			//Loads in current save data
			File LoadSaveFile = new Godot.File();
			LoadSaveFile.Open(this.SavePath, Godot.File.ModeFlags.Read);
			//GD.Print(LoadSaveFile.IsOpen());
			var SaveData = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(LoadSaveFile.GetLine()).Result );
			LoadSaveFile.Close();
			//Modifying the data
			CurrentStage = int.Parse(NextLevel);
			ToLevel = CurrentStage;
			Godot.Collections.Array NewStagePosition = new Godot.Collections.Array(){CurrentLevel, int.Parse(NextLevel)};
			SaveData["WorldData"] = NewStagePosition; 
			//Overwrites save data - alternatively we can just save if the user presses exit from a menu
			//TODO: add a menu that prompts settings when the user presses ESC while in game
			File UpdateSaveFile = new Godot.File();
			UpdateSaveFile.Open(this.SavePath, Godot.File.ModeFlags.Write);
			UpdateSaveFile.StoreString(JSON.Print(SaveData));
			UpdateSaveFile.Close();
			//Set player position
			var StagePositionData = (Godot.Collections.Dictionary)SpawnData[$"{ToLevel}"];
			this.PlayerSpawnPosition = (Godot.Collections.Array)StagePositionData[$"{FromLevel}"];
			// GD.Print(this.PlayerSpawnPosition);

			SceneHelper(CurrentStageScenePath);
		}
	}

	public void StartGame(int SaveFileNum, string Password){
		Godot.File SaveFile = new Godot.File();
		this.SavePath = $"user://save{SaveFileNum}.json";
		// SaveFile.OpenEncryptedWithPass(this.SavePath, Godot.File.ModeFlags.Read, Password);
		SaveFile.Open(this.SavePath, Godot.File.ModeFlags.Read);
		var SaveData = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(SaveFile.GetLine()).Result );
		Godot.Collections.Array StageData = (Godot.Collections.Array)SaveData["WorldData"];
		this.CurrentLevel = Convert.ToInt32(StageData[0]);
		this.CurrentStage = Convert.ToInt32(StageData[1]);
		//Setting pause menu to ON so we can pause in game but not in menu
		Pause PauseMenu = (Pause)GetNode("/root/Pause");
		PauseMenu.On = true;
		// //TODO: Deal with gameplay options here - this is a stub

		// //TODO: Deal with player unlocked abilities here - this is a stub

		string CurrentStageScenePath = $"res://Levels/Level{this.CurrentLevel}/Stages/Stage{this.CurrentStage}.tscn";
		SceneHelper(CurrentStageScenePath);
		SaveFile.Close();
	}

	public void MoveToNewGameMenu(){
		string NewGameMenu = $"res://Non-Levels/tempSaveScreen.tscn";
		SceneHelper(NewGameMenu);
	}

	public void MoveToLoadGameMenu(){
		string LoadGameMenu = $"res://Non-Levels/tempLoadScreen.tscn";
		SceneHelper(LoadGameMenu);
	}

	public void MoveToOptionMenu(){
		//This is a stub - Make option screen later
	}

	public void ReturnToMainMenu(){
		string MainMenuPath = $"res://Non-Levels/tempTitleScreen.tscn";
		SceneHelper(MainMenuPath);
	}

}
