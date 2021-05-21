using Godot;
using System;
using System.Collections.Generic;

public class DeleteConfirm : Popup
{
    [Signal]
    public delegate void ReturnToSaveMenu(bool DeleteDecision);

    private bool AcceptInput = false;

    private MarginContainer ParentContainer;

    private List<string> QuestionPromptPath = new List<string>{
        {"CenterContainer/VBoxContainer/TopLine"},
        {"CenterContainer/VBoxContainer/BottomLine"}
    };

    private List<string> YesNoPath = new List<string>{
        {"CenterContainer/VBoxContainer/HBoxContainer/Left >"},
        {"CenterContainer/VBoxContainer/HBoxContainer/Yes"},
        {"CenterContainer/VBoxContainer/HBoxContainer/Spacing"},
        {"CenterContainer/VBoxContainer/HBoxContainer/Right >"},
        {"CenterContainer/VBoxContainer/HBoxContainer/No"},
    };

    private List<Label> QuestionPrompt = new List<Label>{};
    private List<Label> YesNo = new List<Label>{};

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {   

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(AcceptInput){
            if(Input.IsActionJustPressed("ui_right")){
				YesNo[0].Text = "";
                YesNo[3].Text = ">";
			}
			if(Input.IsActionJustPressed("ui_left")){
				YesNo[0].Text = ">";
                YesNo[3].Text = "";
			}
            if(Input.IsActionJustPressed("ui_cancel")){
                // EmitSignal("ReturnToSaveMenu", false);
                // AcceptInput = false;
            }
            if(Input.IsActionJustPressed("ui_accept")){
                if(YesNo[0].Text == ">"){
                    EmitSignal("ReturnToSaveMenu", true);
                }else{
                    EmitSignal("ReturnToSaveMenu", false);
                }
                AcceptInput = false;
            }
        }

    }

    public void ProcessPopup(int SaveFile){
        ParentContainer = (MarginContainer)GetParent();
        Connect("ReturnToSaveMenu", ParentContainer, "ExecuteDeleteDecision");

        this.PopupCenteredRatio(1.0f);
        QuestionPrompt.Add( (Label)GetNode(QuestionPromptPath[0]) );
        QuestionPrompt.Add( (Label)GetNode(QuestionPromptPath[1]) );

        QuestionPrompt[0].Text = $"Are you sure you";
        QuestionPrompt[1].Text = $"want to delete Save {SaveFile}?";

        foreach(string ElementPath in YesNoPath){
            YesNo.Add( (Label)GetNode(ElementPath) );
        }

        YesNo[0].Text = ">";
        YesNo[1].Text = "Yes";
        YesNo[2].Text = "     ";
        YesNo[3].Text = "";
        YesNo[4].Text = "No";

        AcceptInput = true;
    }

}
