﻿using Assets.FootballGameEngine_Indie.Scripts.Managers;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Managers;
using Assets.FootballGameEngine_Indie.Scripts.Utilities;
using RobustFSM.Base;
using RobustFSM.Interfaces;
using UnityEngine;
using static Assets.FootballGameEngine_Indie.Scripts.Managers.MatchManager;

namespace Assets.FootballGameEngine_Indie_.Scripts.States.Managers.MatchManagerMainState.MatchOn.SubStates.MatchStopped.SubStates
{
    public class BroadcastCornerKick : BState
    {
        /// <summary>
        /// A reference to the wait time
        /// </summary>
        float waitTime;

        public override void Enter()
        {
            base.Enter();

            //set the wait time
            waitTime = 3f;

            //raise the half-start event
            RaiseTheCornerKickEvent();
        }

        public override void Execute()
        {
            base.Execute();

            //decrement the time
            waitTime -= Time.deltaTime;

            //go to wait-for-kick-to-complete if time is less than 0
            if (waitTime <= 0f)
            {
                ((IState)Machine).Machine.ChangeState<WaitForCornerKickToComplete>();
            }
        }

        public override void Exit()
        {
            base.Exit();

            //raise the event that I finished broadcasting the start
            //of the first half
            ActionUtility.Invoke_Action(Owner.OnFinishBroadcastMessage);
        }

        /// <summary>
        /// Raises the half start event
        /// </summary>
        public void RaiseTheCornerKickEvent()
        {
            //prepare an empty string
            string message = "Corner Kick";

            //raise the event
            ActionUtility.Invoke_Action(message, Owner.OnStartBroadcastMessage);
        }

        /// <summary>
        /// Returns the owner of this instance
        /// </summary>
        public MatchManager Owner
        {
            get
            {
                return ((MatchManagerFSM)SuperMachine).Owner;
            }
        }
    }
}
