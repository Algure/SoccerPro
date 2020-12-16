using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.IdleState.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.PutBallBackIntoPlay.MainState;
using RobustFSM.Base;
using UnityEngine;

namespace Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.IdleState.SubStates
{
    public class HoldBall : BState
    {
        public override void Enter()
        {
            base.Enter();

            // register to the OnInstructedToPutBallBackIntoPlay event
            Owner.OnInstructedToPutBallBackIntoPlay += Instance_OnInstructedToPutBallBackIntoPlay;

            // rotate ball to face direction of ball position
            Ball.Instance.transform.rotation = Owner.BallFrontPosition.rotation;
        }

        public override void Execute()
        {
            base.Execute();

            //move the ball to the ball front position
            Ball.Instance.Position = Vector3.Lerp(Ball.Instance.Position,
                                                  Owner.BallFrontPosition.position,
                                                  2f * Time.deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            // deregister to the OnInstructedToPutBallBackIntoPlay event
            Owner.OnInstructedToPutBallBackIntoPlay -= Instance_OnInstructedToPutBallBackIntoPlay;
        }

        public override void OnAnimatorExecuteIK(int layerIndex)
        {
            base.OnAnimatorExecuteIK(layerIndex);
           
            // move the hands to hold the player left and right ball IK targets
            Owner.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            Owner.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);

            // move the hands to the positions
            Owner.Animator.SetIKPosition(AvatarIKGoal.LeftHand, Ball.Instance.BallIkTargets.IkTargetLeft.position);
            Owner.Animator.SetIKPosition(AvatarIKGoal.RightHand, Ball.Instance.BallIkTargets.IkTargetRight.position);
        }

        private void Instance_OnInstructedToPutBallBackIntoPlay()
        {
            SuperMachine.ChangeState<PutBallBackIntoPlayMainState>();
        }

        Player Owner
        {
            get
            {
                return ((GoalKeeperFSM)SuperMachine).Owner;

            }
        }
    }
}
