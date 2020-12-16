using Assets.RobustFSM.Mono;
using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.GoToHome.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.Init.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.InterceptShot.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.Wait.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.InteractWithBall.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.IdleState.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.PutBallBackIntoPlay.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.States.Entities.PlayerStates.GoalKeeperStates.TakeGoalKick.MainState;

namespace Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities
{
    public class GoalKeeperFSM : MonoFSM<Player>
    {
        public override void AddStates()
        {
            base.AddStates();

            // add states
            AddState<GoToHomeMainState>();
            AddState<IdleMainState>();
            AddState<InitMainState>();
            AddState<InterceptShotMainState>();
            AddState<InteractWithBallMainState>();
            AddState<PutBallBackIntoPlayMainState>();
            AddState<TakeGoalKickMainState>();
            AddState<TendGoalMainState>();
            AddState<WaitMainState>();

            // set initial states
            SetInitialState<InitMainState>();
        }
    }
}
