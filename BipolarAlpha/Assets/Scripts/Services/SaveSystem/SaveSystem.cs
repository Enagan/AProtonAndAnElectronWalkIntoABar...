﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SMSceneManagerSystem;

// Class that processes the saving and loading the state of the world
public class SaveSystem : MonoBehaviour
{

  private string _rootPath;
  /// <summary>
  /// Saves the received list of existing room states into .lvl files (in XML format), and saves
  /// the (received) name of the active room, the paths where the room definitions are being saved
  /// and the player's position and rotation into a SaveState class.
  /// Saves the SaveState class as a .lvl file, in XML format.
  /// </summary>
  public void Save(WorldStateDefinition worldState)
  {
    SaveState saveState = new SaveState();

    saveState.activeRoom = worldState.startingRoom;

    List<string> paths = new List<string>();

    foreach (RoomDefinition room in worldState.roomsDefinedInState)
    {
      XMLSerializer.Serialize<RoomDefinition>(room, _rootPath + "Saves/" + room.roomName + ".lvl");
      paths.Add(_rootPath + "Saves/" + room.roomName + ".lvl");
    }

    saveState.roomPaths = paths;

    Transform player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().transform;

    saveState.playerPosition = player.position;
    saveState.playerRotation = player.eulerAngles;

    XMLSerializer.Serialize<SaveState>(saveState, _rootPath + "Saves/SaveState.lvl");
  }

  /// <summary>
  /// Retrieves the last saved world state from an existing .lvl file.  
  /// Resets the player's position and rotation and returns what the active room was and the list of room definitions.
  /// </summary>
  /// <returns></returns>
  private WorldStateDefinition Load(string saveStatePath)
  {
    SaveState saveState = XMLSerializer.Deserialize<SaveState>(saveStatePath);

    if (saveState == null)
    {
      throw new BipolarExceptionSaveStateNotFound("No Save State was found");
    }

    List<RoomDefinition> loadedRooms = new List<RoomDefinition>();

    RoomDefinition savedRoom;

    foreach (string path in saveState.roomPaths)
    {
      savedRoom = XMLSerializer.Deserialize<RoomDefinition>(path);
      loadedRooms.Add(savedRoom);
    }

    Transform player = GameObject.FindGameObjectWithTag("Player").transform;
    player.position = saveState.playerPosition + new Vector3(0, 0.1f, 0);
    player.eulerAngles = saveState.playerRotation;

    return new WorldStateDefinition(loadedRooms, saveState.activeRoom);
  }

  public WorldStateDefinition LoadSaveState()
  {
    return Load(_rootPath + "Saves/SaveState.lvl");
  }

  public WorldStateDefinition LoadInitialState()
  {
    return Load(_rootPath + "SaveState.lvl");
  }

  void Start()
  {
    if (Application.isEditor)
    {
      _rootPath = "Assets/Resources/Levels/";
    }
    else
    {
      _rootPath = Application.dataPath + "/Levels/";
    }

    ServiceLocator.ProvideSaveSystem(this);
  }

  void Update()
  {
    //if (Input.GetKey(KeyCode.End)) { ServiceLocator.GetSceneManager().SaveWorldState(); }
    //if (Input.GetKey(KeyCode.Home)) { ServiceLocator.GetSceneManager().LoadRooms(); }
  }
}
