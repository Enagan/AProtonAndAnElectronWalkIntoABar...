﻿// Made by: Lousada
using UnityEngine;
using System.Collections;

/// <summary>
/// Automatic lids that open when Activated and Close when Deactivated
/// </summary>
public class MagnetDispenserTopLid : MonoBehaviour, Activator
{
  Animation _topLid;

  public void Start()
  {
    _topLid = (Animation)transform.GetComponent(typeof(Animation));
  }

  public void Activate()
  { 
    _topLid.CrossFade("TopLidOpen");
  }

  public void Deactivate()
  {
    _topLid.CrossFade("TopLidClose");
  }
}
