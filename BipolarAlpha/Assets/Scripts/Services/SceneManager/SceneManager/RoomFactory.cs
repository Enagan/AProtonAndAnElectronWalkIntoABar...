//Made By: Engana
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Room Factory is a helper class for the Scene Mananger, 
/// it handles the low level details of instancing new rooms 
/// and correctly placing them in relation to one another
/// </summary>
public class RoomFactory
{
  private RoomFactoryInstancedObjectsRegistry _instancedObjects = new RoomFactoryInstancedObjectsRegistry();

  #region [Public Methods] Room Creation, Destruction and Definition Update
  /// <summary>
  /// Instances a new room. In case the room is already instanced, the function does nothing.
  /// If a "from" room is provided, the new room will be placed connected to the from room
  /// at their shared gateway. In case a gateway doesn't exist between the rooms, the function does nothing.
  /// </summary>
  public void CreateRoom(RoomDefinition newRoom, RoomDefinition fromRoom = null)
  {
    if (!_instancedObjects.RoomIsRegistered(newRoom))
    {
      if (fromRoom == null)
      {
        CreateFirstRoom(newRoom);
      }
      else
      {
        ServiceLocator.GetSceneManager().StartCoroutine(CreateAdjancentRoom(newRoom, fromRoom));

        //CreateAdjancentRoom(newRoom, fromRoom);
      }
    }
  }

  /// <summary>
  /// Updates the definition of a specific room, applying the current state
  /// of all objects to the current definition
  /// </summary>
  /// ENGANA@TODO savestate, as well as velocity and torque
  public RoomDefinition UpdateRoomDefinition(RoomDefinition roomDef)
  {
    if (_instancedObjects.RoomIsRegistered(roomDef))
    {
      List<RoomObjectDefinition> updatedDefs = new List<RoomObjectDefinition>();
      List<RoomObjectGatewayDefinition> updatedGates= new List<RoomObjectGatewayDefinition>();
      
      foreach (KeyValuePair<RoomObjectDefinition, GameObject> objs in _instancedObjects.GetAllGameObjectsFromRoom(roomDef))
      {
          List<ComplexState> updatedComplexStates = new List<ComplexState>();
          foreach (ComplexState complexState in objs.Key.complexStates)
          {
            Transform objectWithComplexState = objs.Value.transform.Find(complexState.objectNameInHierarchy);
            IHasComplexState scriptToLoadComplexState = (objectWithComplexState.GetComponent(complexState.GetComplexStateName()) as IHasComplexState);
            updatedComplexStates.Add(scriptToLoadComplexState.UpdateComplexState(complexState));
          }

          objs.Key.position = objs.Value.transform.position;
          objs.Key.eulerAngles = objs.Value.transform.eulerAngles;
          objs.Key.complexStates = updatedComplexStates;

          if (!(objs.Key is RoomObjectGatewayDefinition))
          {
            updatedDefs.Add(objs.Key);
          }
          else
          {
            updatedGates.Add(objs.Key as RoomObjectGatewayDefinition);
          }
      }

      roomDef.objectsInRoom = updatedDefs;
      roomDef.gateways = updatedGates;

      return roomDef;
    }
    else
    {
      BipolarConsole.AllLog("Error: Updating room " + roomDef.roomName + " failed. Room does not exist in registry");
      return null;
    }
  }

  /// <summary>
  /// Changes an objects parent room in the current registry while updating transform.parent as well
  /// </summary>
  public void ChangeObjectRoom(RoomDefinition prevRoom, RoomDefinition newRoom, GameObject objectChangedRoom)
  {
    objectChangedRoom.transform.parent = _instancedObjects.getRoomParentObject(newRoom).transform;

    RoomObjectDefinition objectDef = _instancedObjects.GetDefinitionFromGameObject(prevRoom, objectChangedRoom);
    _instancedObjects.UnregisterObjectFromRoom(prevRoom, objectDef);
    _instancedObjects.RegisterObjectInRoom(newRoom, objectDef, objectChangedRoom);
  }

  /// <summary>
  /// Destroys an already instanced room. In case the room in non-existant, an error is launched
  /// </summary>
  public void DestroyRoom(RoomDefinition roomDef)
  {
    if (_instancedObjects.RoomIsRegistered(roomDef))
    {
      GameObject toDestroy = _instancedObjects.getRoomParentObject(roomDef);
      _instancedObjects.RemoveRoomFromRegistry(roomDef);
      GameObject.Destroy(toDestroy);
    }
    else
    {
      BipolarConsole.AllLog("Error: Deletion of room " + roomDef.roomName + " failed. Room does not exist in registry");
    }
  }
  #endregion

  #region [Private] Room Creation Auxiliary functions
  /// <summary>
  /// Creates room as the original room, centered at the world origin
  /// </summary>
  private void CreateFirstRoom(RoomDefinition room)
  {
    //Creates the room parent object
    GameObject roomParentObject = new GameObject(room.roomName);
    roomParentObject.SetActive(false);
    _instancedObjects.RegisterRoom(room, roomParentObject);

    //Instances all objects present in the room definition 
    foreach (RoomObjectDefinition obj in room.objectsInRoom)
    {
      GameObject instancedObject = InstanceObject(obj, roomParentObject.transform, Vector3.zero);

      _instancedObjects.RegisterObjectInRoom(room, obj, instancedObject);
    }

    //Instances all gateways in the room definition
    foreach (RoomObjectGatewayDefinition gate in room.gateways)
    {
      GameObject instancedObject = InstanceObject(gate, roomParentObject.transform, Vector3.zero);

      _instancedObjects.RegisterObjectInRoom(room, gate, instancedObject);
    }

    roomParentObject.SetActiveRecursively(true);
  }

  /// <summary>
  /// Creates new room adjacent to the "from" room.
  /// Rooms will be connected via their respective gateways to each other.
  /// Execution will halt in case a connection does not exist.
  /// </summary>
  private IEnumerator CreateAdjancentRoom(RoomDefinition newRoom, RoomDefinition from)
  {
    RoomObjectGatewayDefinition fromGate;
    RoomObjectGatewayDefinition newRoomGate;

    //Retrives the gateways between rooms.
    //TODO Exceptioning
    if ((fromGate = from.GetGatewayTo(newRoom)) == null)
    {
      BipolarConsole.AllLog("Error: Gateway between rooms " + from.roomName + " and " + newRoom.roomName + " not found");
      yield break;
      //return;
    }
    if ((newRoomGate = newRoom.GetGatewayTo(from)) == null)
    {
      BipolarConsole.AllLog("Error: Gateway between rooms " + newRoom.roomName + " and " + from.roomName + " not found");
      yield break;
      //return;
    }

    //Creates the room parent object
    GameObject roomParentObject = new GameObject(newRoom.roomName);
    roomParentObject.SetActive(false);
    _instancedObjects.RegisterRoom(newRoom, roomParentObject);

    //Orients the parent object to the new room gateway, as their centers will coincide
    roomParentObject.transform.eulerAngles = newRoomGate.eulerAngles;

    //Instances all gateways in room definition, in a position relative 
    //to the newRoomGate (The local origin)
    foreach (RoomObjectGatewayDefinition gate in newRoom.gateways)
    {
      GameObject instancedObject = InstanceObject(gate, roomParentObject.transform, newRoomGate.position);

      _instancedObjects.RegisterObjectInRoom(newRoom, gate, instancedObject);
    }

    //Instances all objects in room definition, in a position relative 
    //to the newRoomGate (The local origin)
    foreach (RoomObjectDefinition obj in newRoom.objectsInRoom)
    {
      GameObject instancedObject = InstanceObject(obj, roomParentObject.transform, newRoomGate.position);

      _instancedObjects.RegisterObjectInRoom(newRoom, obj, instancedObject);
      yield return null;
    }

    //Retrive the "from" rooms' gate position and rotation, as these will be the starting position of the new room
    Vector3 fromGateWorldPosition = _instancedObjects.GetGameObjectFromDefinition(from, fromGate).transform.position;
    Vector3 fromGateWorldRotation = _instancedObjects.GetGameObjectFromDefinition(from, fromGate).transform.eulerAngles;

    //Positions and orients the parent object to match and connect with the from room gateway
    roomParentObject.transform.position = fromGateWorldPosition;
    roomParentObject.transform.eulerAngles = OppositeVector(fromGateWorldRotation);

    roomParentObject.SetActiveRecursively(true);
  }

  /// <summary>
  /// Instances an object from an object definition, assigning him a parent and positioning 
  /// him relative to the relativeOrigin provided, in case these arguments are used
  /// </summary>
  private GameObject InstanceObject(RoomObjectDefinition obj, Transform parentTransform = null, Vector3 relativeOrigin = default(Vector3))
  {

    GameObject instancedObject = ServiceLocator.GetResourceSystem().InstanceOf(obj.objectPrefabPath,active:false);

    instancedObject.transform.localPosition = WorldPositionInRelationTo(obj.position, relativeOrigin);
    instancedObject.transform.localScale = obj.scale;
    instancedObject.transform.localEulerAngles = obj.eulerAngles;

    instancedObject.transform.parent = parentTransform;

    foreach (ComplexState complexState in obj.complexStates)
    {
      Transform objectWithComplexState = instancedObject.transform.Find(complexState.objectNameInHierarchy);
      IHasComplexState scriptToLoadComplexState = (objectWithComplexState.GetComponent(complexState.GetComplexStateName()) as IHasComplexState);
      scriptToLoadComplexState.LoadComplexState(complexState);
    }

    return instancedObject;
  }

  /// <summary>
  /// Returns a position in relation of another position
  /// </summary>
  private Vector3 WorldPositionInRelationTo(Vector3 originalObjectPosition,
    Vector3 localRelationalObjectPositon)
  {
    return (originalObjectPosition - localRelationalObjectPositon);
  }

  /// <summary>
  /// Returns the opposite vector, keeping the up vector intact
  /// </summary>
  private Vector3 OppositeVector(Vector3 vec)
  {
    return new Vector3(-vec.x, vec.y + 180, -vec.z);
  }

  #endregion

  
}
