using Godot;
using System;
using System.Collections.Generic;

public class Config : Node
{
	public Godot.Collections.Array PlayerSpawnPosition = null;

	public void SetPlayerSpawn(Godot.Collections.Array SpawnLocation){
		this.PlayerSpawnPosition = SpawnLocation;
	}
}
