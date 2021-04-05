using Godot;
using System;

public class Zone_A : Area2D
{
    [Signal]
    public delegate void ZoneAEntered(string ZoneLetter);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SceneHandler SCNHAND = (SceneHandler)GetNode("/root/SceneHandler");
        GD.Print(SCNHAND.GetType());
        Connect("ZoneAEntered", SCNHAND, "ChangeStage");
    }

    public override void _Process(float delta)
    {
        var OverlappingBodies = this.GetOverlappingBodies();
        if(OverlappingBodies.Count > 0){
            foreach(var body in OverlappingBodies){
                if( (body as Player) != null){
                    //Make sure the letter in the signal corresponds to the zone name
                    //Also make sure the letter exists in the JSON for the level progression
                    EmitSignal("ZoneAEntered", "A");
                }
            }
        }
    }
}
