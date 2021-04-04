using Godot;
using System;
using System.Collections.Generic;

public class tempSaveScreen : MarginContainer
{
    [Signal]
    public delegate void StartGame(int SaveFileNum, string Password);
    [Signal]
    public delegate void GoToMainMenu();

    //This is the password we use to encrypt the json storing the game/save data
    private string Password = "yeaaa yeaaa";
    private string DefaultDataPath = "res://Data/DefaultData.json";

    //Paths in the scene to the arrows that move
    private List<string> SavePaths = new List<string>{
        {"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/>"},
        {"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/>"},
        {"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/>"}
    };

    //Paths in the scene to the save file labels
    private List<string> SaveFileDescs = new List<string>{
        {"CenterContainer/VBoxContainer/Save 1 Parent/HBoxContainer/Save 1"},
        {"CenterContainer/VBoxContainer/Save 2 Parent/HBoxContainer/Save 2"},
        {"CenterContainer/VBoxContainer/Save 3 Parent/HBoxContainer/Save 3"}
    };

    //Contains the labels we modify to change text
    private List<Label> SaveList = new List<Label>();
    private List<Label> SaveFileDescList = new List<Label>();

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
                SaveList[_OptionNumber].Text = "";
                _OptionNumber = value;
                SaveList[_OptionNumber].Text = ">";
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Getting the autoload scene handler and connecting signals
        SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
        if(SCNHAND == null){
            throw new ArgumentNullException("Edwin: Autoload SceneHandler not found"); 
        }
        Connect("StartGame", SCNHAND, "StartGame");
        Connect("GoToMainMenu", SCNHAND, "ReturnToMainMenu");
        //Getting the labels/save files from the label/save file paths from within the scene
        MaxOptionLength = SavePaths.Count;
        foreach(string SavePath in SavePaths){
            SaveList.Add(GetNode(SavePath) as Label);
        }
        foreach(string SaveFileDesc in SaveFileDescs){
            SaveFileDescList.Add(GetNode(SaveFileDesc) as Label);
        }
        SaveList[OptionNumber].Text = ">";
        //Adding the text to the save files - whether empty or at some stage
        File SaveFile = new Godot.File();
        for(int i = 0; i < SaveFileDescs.Count; i++){
            int SaveNum = i + 1;
            SaveFileDescList[i].Text = $"Save {SaveNum}";
            if(! SaveFile.FileExists($"user://save{SaveNum}.json") ){
                SaveFileDescList[i].Text += $" - Empty Save";
            }else{
                string SavePath = $"user://save{SaveNum}.json";
                File LoadDefaultData = new Godot.File();
                //LoadDefaultData.OpenEncryptedWithPass(SavePath, Godot.File.ModeFlags.Read, Password);
                LoadDefaultData.Open(SavePath, Godot.File.ModeFlags.Read);
                var DefaultList = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(LoadDefaultData.GetLine()).Result );
                LoadDefaultData.Close();

                Godot.Collections.Array StageData = (Godot.Collections.Array)DefaultList["WorldData"];
                SaveFileDescList[i].Text += $" - Stage {StageData[0]}-{StageData[1]}";
            }
        }
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
            CreateSave(OptionNumber + 1);
        }
        if(Input.IsActionJustPressed("ui_cancel")){
            EmitSignal("GoToMainMenu");
        }
    }

    //Creates a new save file and starts the game with default values
    //TODO: Overwrite prompt
    public void CreateSave(int SaveFileNum){
        File SaveFile = new Godot.File();
        string SavePath = $"user://save{SaveFileNum}.json";
        if( SaveFile.FileExists($"user://save{SaveFileNum}.json") ){
            //This is a stub
            //Instance new scene to prompt if user wants to overwrite
        }else{
            //Creates a save and loads a new game with default data
            //TODO - CALCULATE A HASH FOR THE DEFAULT JSON DATA AND COMPARE THE HASHES
            File LoadDefaultData = new Godot.File();
            LoadDefaultData.Open(DefaultDataPath, Godot.File.ModeFlags.Read);
            var DefaultList = new Godot.Collections.Dictionary<string, object>( (Godot.Collections.Dictionary)JSON.Parse(LoadDefaultData.GetLine()).Result );
            LoadDefaultData.Close();

            //SaveFile.OpenEncryptedWithPass(SavePath, Godot.File.ModeFlags.Write, Password);
            SaveFile.Open(SavePath, Godot.File.ModeFlags.Write);
            SaveFile.StoreString(JSON.Print(DefaultList));
            SaveFile.Close();

            EmitSignal("StartGame", SaveFileNum, Password);
        }
    }

}
