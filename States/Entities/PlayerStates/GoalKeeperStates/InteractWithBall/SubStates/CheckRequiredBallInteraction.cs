using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.InteractWithBall.MainState;
using RobustFSM.Base;

namespace Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.InteractWithBall.SubStates
{
    public class CheckRequiredBallInteraction : BState
    {
        bool _hasResetPunchBallState;

        public override void Enter()
        {
            base.Enter();

            // set some variables
            _hasResetPunchBallState = false;

            // check if ball is catchable and 
            // proceed to the appropriate state
            bool isBallCatchable = ((InteractWithBallMainState)Machine).InterceptShotMainState.IsBallCatchable;
            bool isBallInteractable = ((InteractWithBallMainState)Machine).InterceptShotMainState.IsBallInteractable;

            // if ball is catchable then catch the ball else punch it
            if (isBallInteractable)
            {
                if (isBallCatchable)
                    Machine.ChangeState<CatchBall>();
                else
                    Machine.ChangeState<PunchBall>();
            }
            else
            {
                //set the animator to exit the dive state
                Owner.Animator.SetTrigger("Exit");
            }
        }


        public override void Execute()
        {
            base.Execute();

            if(_hasResetPunchBallState == false)
            {
                if (Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("PunchBall"))
                {
                    _hasResetPunchBallState = true;

                    Owner.Animator.SetTrigger("Exit");
                }
            }
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
