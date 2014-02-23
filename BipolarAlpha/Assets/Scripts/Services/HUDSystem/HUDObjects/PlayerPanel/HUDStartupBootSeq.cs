﻿using UnityEngine;
using System.Collections;

public class HUDStartupBootSeq : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ServiceLocator.GetHUDSystem().StartBootUpSequence();
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel("Main");

        }
	}
}