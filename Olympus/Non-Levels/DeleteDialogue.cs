using Godot;
using System;
using System.Collections.Generic;

public class DeleteDialogue : CanvasLayer
{
    [Signal]
    public delegate void ReturnToPlayMenu(bool Action);

    public bool On = false;
    public bool InnerOn = false;

    MarginContainer PlayScreen = null;
    MarginContainer PromptScreen;

    private List<string> QuestionPromptPath = new List<string>{
        {"PromptScreen/CenterContainer/VBoxContainer/TopLine"},
        {"PromptScreen/CenterContainer/VBoxContainer/BottomLine"}
    };

    private List<string> YesNoPath = new List<string>{
        {"PromptScreen/CenterContainer/VBoxContainer/HBoxContainer/Left >"},
        {"PromptScreen/CenterContainer/VBoxContainer/HBoxContainer/Yes"},
        {"PromptScreen/CenterContainer/VBoxContainer/HBoxContainer/Spacing"},
        {"PromptScreen/CenterContainer/VBoxContainer/HBoxContainer/Right >"},
        {"PromptScreen/CenterContainer/VBoxContainer/HBoxContainer/No"},
    };

    private List<Label> QuestionPrompt = new List<Label>{};
    private List<Label> YesNo = new List<Label>{};

    public override void _Ready()
    {
        //Getting the labels/save files from the label/save file paths from within the scene
        QuestionPrompt.Add( (Label)GetNode(QuestionPromptPath[0]) );
        QuestionPrompt.Add( (Label)GetNode(QuestionPromptPath[1]) );

        QuestionPrompt[0].Text = $"Are you sure you";
        QuestionPrompt[1].Text = $"want to delete Save 42069?";

        foreach(string ElementPath in YesNoPath){
            YesNo.Add( (Label)GetNode(ElementPath) );
        }

        YesNo[0].Text = ">";
        YesNo[1].Text = "Yes";
        YesNo[2].Text = "     ";
        YesNo[3].Text = "";
        YesNo[4].Text = "No";

        PromptScreen = (MarginContainer)GetNode("PromptScreen");
        PromptScreen.Visible = false;
    }

    public override void _Process(float delta)
    {
        if(this.On){
            if(Input.IsActionJustReleased("ui_accept")){
                this.InnerOn = true;
            }
            if(this.InnerOn){
                if(Input.IsActionJustPressed("ui_right")){
                    YesNo[0].Text = "";
                    YesNo[3].Text = ">";
                }
                if(Input.IsActionJustPressed("ui_left")){
                    YesNo[0].Text = ">";
                    YesNo[3].Text = "";
                }
                if(Input.IsActionJustPressed("ui_accept")){
                    PromptScreen.Visible = false;
                    GetTree().Paused = false;
                    if(YesNo[0].Text == ">"){
                        EmitSignal("ReturnToPlayMenu", true);
                    }else
                    if(YesNo[3].Text == ">"){
                        EmitSignal("ReturnToPlayMenu", false);
                    }
                }
                if(Input.IsActionJustPressed("ui_cancel")){
                    PromptScreen.Visible = false;
                    GetTree().Paused = false;
                    EmitSignal("ReturnToPlayMenu", false);
                }
            }
        }
    }

    public void RaisePrompt(int SaveFileNum){
        YesNo[0].Text = ">";
        YesNo[1].Text = "Yes";
        YesNo[2].Text = "     ";
        YesNo[3].Text = "";
        YesNo[4].Text = "No";
        GD.Print("Raise prompt");
        if(PlayScreen == null){
            GD.Print("play screen null");
            PlayScreen = (MarginContainer)GetParent();
            Connect("ReturnToPlayMenu", PlayScreen, "GetConfirmation");
        }
        QuestionPrompt[1].Text = $"want to delete Save {SaveFileNum}?";
        PromptScreen.Visible = true;
        if(PromptScreen.Visible = true){

        }
        GetTree().Paused = true;
        this.On = true;
        GD.Print("End prmopt");
    }
}
