using Godot;
using System;
using System.Collections.Generic;

public class tempTitleScreen : MarginContainer
{
    [Signal]
    public delegate void OpenNewGame();
    [Signal]
    public delegate void OpenLoadGame();
    [Signal]
    public delegate void OpenGameOptions();

    //Paths to the labels we modify
    private List<string> LabelPaths = new List<string>{
        {"CenterContainer/VBoxContainer/Select/VBoxContainer/New Game/HBoxContainer/>"}, //New Game
        {"CenterContainer/VBoxContainer/Select/VBoxContainer/Load Game/HBoxContainer/>"}, //Load Game
        {"CenterContainer/VBoxContainer/Select/VBoxContainer/Options/HBoxContainer/>"}, //Options
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
        Connect("OpenNewGame", SCNHAND, "MoveToNewGameMenu");
        Connect("OpenLoadGame", SCNHAND, "MoveToLoadGameMenu");
        Connect("OpenGameOptions", SCNHAND, "MoveToOptionMenu");

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
                    NewGame();
                    break;
                case 1:
                    LoadGame();
                    break;
                case 2:
                    Options();
                    break;
                case 3:
                    Exit();
                    break;
                default:
                    break;
            }
        }
    }

    private void NewGame(){
        EmitSignal("OpenNewGame");
    }

    private void LoadGame(){
        EmitSignal("OpenLoadGame");
    }

    private void Options(){
        EmitSignal("OpenGameOptions");
    }

    private void Exit(){
        GetTree().Quit();
    }
}
