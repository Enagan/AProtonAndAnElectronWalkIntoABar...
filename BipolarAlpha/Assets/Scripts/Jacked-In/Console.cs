﻿using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour {

  private GameObject _realPlayer;
  private GameObject _jackedInPlayer = null;
  private float _playerMass;

  [SerializeField]
  private float waitingTime = 2.0f;

  void OnCollisionEnter(Collision col)
  {
    if (col.gameObject.tag == "Player" && _jackedInPlayer == null)
    {
      _realPlayer = col.gameObject;
      ActivateJackedIn();
    }
  }

  /// <summary>
  /// Activates jacked in mode, the player is deactivated and a JackedInPlayer prefab is created
  /// </summary>
  private void ActivateJackedIn() {
    _realPlayer.rigidbody.velocity = Vector3.zero;
    PlayerActivation(false);
    PlayInAnimation();
    this.transform.Find("Boundary").gameObject.SetActive(true);
    _jackedInPlayer = ServiceLocator.GetResourceSystem().InstanceOf("Prefabs/JackedIn/JackedInPlayer", this.transform.position);
    _jackedInPlayer.GetComponent<JackedInPlayer>().MotherConsole = this;
    _jackedInPlayer.transform.forward = -1.0f * _realPlayer.transform.forward;
  }


  /// <summary>
  /// Enables or Disables all the components in the player that can't co-exist with the JackedInPlayer
  /// </summary>
  /// <param name="state"></param>
  private void PlayerActivation(bool state)
  {
    _realPlayer.GetComponent<PlayerController>().enabled = state;

    foreach (Camera cam in _realPlayer.GetComponentsInChildren<Camera>())
    {
      cam.enabled = state;
    }

    foreach (AudioListener audio in _realPlayer.GetComponentsInChildren<AudioListener>())
    {
      audio.enabled = state;
    }


    /// this mass managemente is so the Player won't get pushed around by the JackedInPlayer
    if (!state)
    {
      _playerMass = _realPlayer.GetComponent<Rigidbody>().mass;
      _realPlayer.GetComponent<Rigidbody>().mass = 1000.0f;
    }
    else
    { 
      _realPlayer.GetComponent<Rigidbody>().mass = _playerMass;
  }
  }

  /// <summary>
  /// Deletes the instanced JackedInPlayer and restores the player to an active state
  /// </summary>
  public void DeleteSpawn()
  {
    PlayOutAnimation();
    PlayerActivation(true);
    GameObject.Destroy(_jackedInPlayer);
    _jackedInPlayer = null;
    this.transform.Find("Boundary").gameObject.SetActive(false);

  }



  //Animations or effects of player switching go here

   private void PlayInAnimation(){}
   private void PlayOutAnimation(){}

}
