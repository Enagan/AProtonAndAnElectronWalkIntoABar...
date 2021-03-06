﻿using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour {

  public GameObject _realPlayer;
  public GameObject _jackedInSpawnPoint = null;
  private GameObject _jackedInPlayer = null;
  private float _playerMass;


  /// <summary>
  /// Activates jacked in mode, the player is deactivated and a JackedInPlayer prefab is created
  /// </summary>
  public void ActivateJackedIn() {
    _realPlayer = GameObject.Find("Player");
    _realPlayer.rigidbody.velocity = Vector3.zero;
    _realPlayer.GetComponent<PlayerController>().PlayerActivation(false);
    PlayInAnimation();
    this.transform.Find("Pushy Boundary").gameObject.SetActive(true);
    this.transform.Find("Visual Boundary").gameObject.SetActive(true);
    this.transform.Find("SafeGuard Boundary").gameObject.SetActive(true);

    _jackedInPlayer = ServiceLocator.GetResourceSystem().InstanceOf("Prefabs/JackedIn/JackedInPlayer", _jackedInSpawnPoint != null ? 
                                                                                                              _jackedInSpawnPoint.transform.position :
                                                                                                              this.transform.position);
    _jackedInPlayer.GetComponent<JackedInPlayer>().MotherConsole = this;
    _jackedInPlayer.transform.forward = -1.0f * _realPlayer.transform.forward;

    ServiceLocator.GetEventHandlerSystem().SendJackedInActivationEvent(_jackedInPlayer.GetComponentInChildren<Camera>());
  }




  /// <summary>
  /// Deletes the instanced JackedInPlayer and restores the player to an active state
  /// </summary>
  public void DeleteSpawn()
  {
    PlayOutAnimation();
    _realPlayer.GetComponent<PlayerController>().PlayerActivation(true);
    GameObject.Destroy(_jackedInPlayer);
    _jackedInPlayer = null;
    this.transform.Find("Pushy Boundary").gameObject.SetActive(false);
    this.transform.Find("Visual Boundary").gameObject.SetActive(false);
    this.transform.Find("SafeGuard Boundary").gameObject.SetActive(false);
    ServiceLocator.GetEventHandlerSystem().SendJackedInDeactivationEvent();
  }



  //Animations or effects of player switching go here

   private void PlayInAnimation(){}
   private void PlayOutAnimation(){}

}
