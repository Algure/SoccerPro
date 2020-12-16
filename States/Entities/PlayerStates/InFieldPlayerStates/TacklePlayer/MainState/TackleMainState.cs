using System;
using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.InFieldPlayerStates.ControlBall.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.InFieldPlayerStates.GoToHome.MainState;
using Assets.FootballGameEngine_Indie.Scripts.Utilities;
using RobustFSM.Base;
using UnityEngine;
using static Assets.FootballGameEngine_Indie.Scripts.Entities.Player;

namespace Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.InFieldPlayerStates.TacklePlayer.MainState
{
    public class TackleMainState : BState
    {
        bool _hasReachedTarget;
        bool _hasExitedFinishTackleState;
        bool _isTackleSuccessful;
        float _waitTime;
        Vector3 _target;
        FinishTackleAnimBehaviour _finishTackleAnimBehaviour;

        public override void Initialize()
        {
            base.Initialize();

           // _finishTackleAnimBehaviour = Owner.Animator.GetBehaviour<FinishTackleAnimBehaviour>();
            //_finishTackleAnimBehaviour.OnExit += Instance_OnExit;
        }

        private void Instance_OnExit()
        {
            _hasExitedFinishTackleState = true;
        }

        public override void Enter()
        {
            base.Enter();

            //set the wait time
            _waitTime = 0.1f;

            //randomly find who will win this tackle
            _hasExitedFinishTackleState = false;
            _hasReachedTarget = false;
            _isTackleSuccessful = true;// UnityEngine.Random.value <= 0.5f;

            //if tackle is successful, then message the ball owner
            //that he has been tackled
            if(_isTackleSuccessful)
                ActionUtility.Invoke_Action(Ball.Instance.CurrentOwner.OnTackled);

            //// find the tackle target
            _target = Ball.Instance.NormalizedPosition - Owner.Position;
            _target = Owner.Position + (_target.normalized * (_target.magnitude + 1));

            //// set steering
            Owner.RPGMovement.SetMoveTarget(_target);
            Owner.RPGMovement.SetRotateFacePosition(_target);
            Owner.RPGMovement.SetSteeringOn();
            Owner.RPGMovement.SetTrackingOn();

            // set the animator
            Owner.Animator.SetTrigger("Idle");
            Owner.Animator.SetTrigger("Tackle");
        }

        public override void Execute()
        {
            base.Execute();
            
            // put ball infront of me if tackle successful
            if (_isTackleSuccessful && Owner.IsBallWithinControllableDistance())
                Owner.PlaceBallInfronOfMe();

            if (Owner.IsAtTarget(_target) && _hasReachedTarget == false)
            {
                // set has reached target
                _hasReachedTarget = true;

                // trap/stop the ball
                if (_isTackleSuccessful && Owner.IsBallWithinControllableDistance())
                    Ball.Instance.Trap();

                // turn off steering
                Owner.RPGMovement.SetSteeringOff();
                Owner.RPGMovement.SetTrackingOff();

                //exit the start tackle state
                Owner.Animator.SetTrigger("ExitStartTackle");
            }

            // if the player has gone back to New State then he can go back to another state
            if (_hasExitedFinishTackleState)
            {
                // decrement time
                _waitTime -= Time.deltaTime;

                ///if time if exhausted trigger approprite state transation
                if (_waitTime <= 0)
                {
                    if (_isTackleSuccessful)
                    {
                        // raise event that I'm controlling the ball
                        ControlBallDel temp = Owner.OnControlBall;
                        if (temp != null)
                            temp.Invoke(Owner);

                        // 
                        SuperMachine.ChangeState<ControlBallMainState>();
                    }
                    else
                        SuperMachine.ChangeState<GoToHomeMainState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            ////set the steering to on
            Owner.RPGMovement.SetSteeringOff();
            Owner.RPGMovement.SetTrackingOff();

            //// reset the animator
            Owner.Animator.ResetTrigger("ExitStartTackle");
            Owner.Animator.ResetTrigger("Idle");
            Owner.Animator.ResetTrigger("Tackle");
        }

        public Player Owner
        {
            get
            {
                return ((InFieldPlayerFSM)SuperMachine).Owner;
            }
        }

    }
}
