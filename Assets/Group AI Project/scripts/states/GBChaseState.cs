﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GBChaseState : State {

    public GBChaseState(GameBabyStateController stateController) : base(stateController) { }

    public override void CheckTransitions()
    {
        if (!stateController.CheckIfInRange("Player"))
        {
            stateController.SetState(new GBCryState(stateController));
        }
    }
    public override void Act()
    {
        if(stateController.enemyToChase != null)
        {
            stateController.destination = stateController.enemyToChase.transform;
            stateController.ai.SetTarget(stateController.destination);
        }
    }
    public override void OnStateEnter()
    {
        stateController.ChangeColor(Color.red);
        stateController.ai.agent.speed = .5f;
    }
}
