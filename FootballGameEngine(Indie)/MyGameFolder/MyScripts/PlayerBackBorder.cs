﻿using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBackBorder : MonoBehaviour
{
    MatchManager matchManager;
    Player thisPlayer;
    // Start is called before the first frame update
    void Start()
    {
        matchManager = GameObject.Find("Manager").GetComponent<MatchManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        thisPlayer = GetComponentInParent<Player>();
        Player collPlayer = other.GetComponentInParent<Player>();
        if (collPlayer == null)
        {
            return;
        }
        if (collPlayer.teamChoice == thisPlayer.teamChoice)
        {
            moveForward();
        }
        else
        {
            thisPlayer.TryToPassBall();
        }

    }

    private void moveForward()
    {
        thisPlayer.verticalOffset = Statics.offsetDistance;
    }
}
