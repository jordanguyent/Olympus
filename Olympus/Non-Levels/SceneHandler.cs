using Godot;
using System;
using System.Collections.Generic;

public class SceneHandler : Node
{
	[Signal]
	public delegate void SetPlayerSpawn(Godot.Collections.Array StagePos);

	//Password for file encryption
	public const string Password = "yeaaa yeaaa";

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

	public Godot.Collections.Dictionary<string, object> JSONToDict(string Path){
		Godot.File PathFile = new Godot.File();
		PathFile.Open(Path, Godot.File.ModeFlags.Read);
		var Data = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary) JSON.Parse(PathFile.GetLine()).Result );
		PathFile.Close();
		return Data;
	}

	public override void _Ready(){
		Config CONFIG = (Config)GetNode("/root/Config");
		if (CONFIG == null)
		{
			throw new ArgumentNullException("Edwin: Autoload Config not found"); 
		}
		Connect("SetPlayerSpawn",CONFIG,"SetPlayerSpawn");
	}

	public void ChangeStage(string ZoneEntered){
		//Get which stage to load next
		string ZonePath = $"res://Levels/Level{this.CurrentLevel}/Level{this.CurrentLevel}Zone.json";
		var ZoneData = JSONToDict(ZonePath);
		var StageData = (Godot.Collections.Dictionary)ZoneData[$"{CurrentStage}"];
		string NextLevel = (string)StageData[ZoneEntered];

		//Get where to load in that stage
		string SpawnPath = $"res://Levels/Level{this.CurrentLevel}/Level{this.CurrentLevel}Spawn.json";
		var SpawnData = JSONToDict(SpawnPath);

		//Use the term "NextWorld" in the json to indicate moving from level 1 to level 2 as an example
		//NOTE: LEVELS =/= STAGES
		if(NextLevel == "NextWorld"){

		}else
		if(NextLevel == "PreviousWorld"){

		}else{
			//Stores the variables used for checking spawn point
			int FromLevel = CurrentStage;
			int ToLevel;
			//Gets the string for the next scene since we will modify some values
			string NextStagePath = $"res://Levels/Level{this.CurrentLevel}/Stages/Stage{NextLevel}.tscn";
			//Loads in current save data
			var SaveData = JSONToDict(this.SavePath);
			//Modifying the data in the SceneHandler
			CurrentStage = int.Parse(NextLevel);
			ToLevel = CurrentStage;
			SaveData["WorldData"] = new Godot.Collections.Array(){CurrentLevel, int.Parse(NextLevel)}; 
			
			//Overwrites save data - alternatively we can just save if the user presses exit from a menu
			//TODO: add a menu that prompts settings when the user presses ESC while in game
			File UpdateSaveFile = new Godot.File();
			UpdateSaveFile.Open(this.SavePath, Godot.File.ModeFlags.Write);
			UpdateSaveFile.StoreString(JSON.Print(SaveData));
			UpdateSaveFile.Close();

			//Config fie will handle
			var StagePositionData = (Godot.Collections.Dictionary)SpawnData[$"{ToLevel}"];
			EmitSignal("SetPlayerSpawn", (Godot.Collections.Array)StagePositionData[$"{FromLevel}"]);

			//Load next level :)
			SceneHelper(NextStagePath);
		}
	}

	public void StartGame(int SaveFileNum){
		Godot.File SaveFile = new Godot.File();
		this.SavePath = $"user://save{SaveFileNum}.json";
		// SaveFile.OpenEncryptedWithPass(this.SavePath, Godot.File.ModeFlags.Read, this.Password);
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
		// This is already opened so don't close it since we're not done with it
//		EmitSignal(SaveFile);

		string CurrentStageScenePath = $"res://Levels/Level{this.CurrentLevel}/Stages/Stage{this.CurrentStage}.tscn";
		SceneHelper(CurrentStageScenePath);
		SaveFile.Close();
	}

	public void MoveToPlayGame(){
		string PlayScreenMenu = $"res://Non-Levels/tempPlayScreen.tscn";
		SceneHelper(PlayScreenMenu);
	}

	public void MoveToKeybinds(){
		
	}

	public void MoveToGameOptions(){
		string LoadOptionMenu = $"res://Non-Levels/tempOptionScreen.tscn";
		SceneHelper(LoadOptionMenu);
	}

	public void ReturnToMainMenu(){
		string MainMenuPath = $"res://Non-Levels/tempTitleScreen.tscn";
		SceneHelper(MainMenuPath);
	}

}
