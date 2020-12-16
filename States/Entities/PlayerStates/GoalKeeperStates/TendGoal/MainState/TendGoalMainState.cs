using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.InterceptShot.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.Wait.MainState;
using Assets.FootballGameEngine_Indie.Scripts.Triggers;
using RobustFSM.Base;
using RobustFSM.Interfaces;
using UnityEngine;

namespace Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.GoalKeeperStates.GoToHome.MainState
{
    /// <summary>
    /// The keeper tends/protects the goal from the opposition
    /// </summary>
    public class TendGoalMainState : BState
    {
        int _goalLayerMask;
        float _timeSinceLastUpdate;
        Vector3 _steeringTarget;
        Vector3 _prevBallPosition;

        public override void Initialize()
        {
            base.Initialize();

            // set the goal trigger mask
            _goalLayerMask = LayerMask.GetMask("GoalTrigger");
        }

        public override void Enter()
        {
            base.Enter();

            //set the animator
            Owner.Animator.SetTrigger("TendGoal");

            //set some data
            _prevBallPosition = 1000 * Vector3.one;
            _timeSinceLastUpdate = 0f;

            //set the rpg movement
            _steeringTarget = Owner.Position;
            Owner.RPGMovement.SetSteeringOff();
            Owner.RPGMovement.Speed = Owner.TendGoalSpeed;

            //register to some events
            Owner.OnShotTaken += Instance_OnShotTaken;
            Owner.OnInstructedToWait += Instance_OnWait;
        }

        public override void Execute()
        {
            base.Execute();

#if UNITY_EDITOR
            Debug.DrawLine(Owner.Position + Vector3.up, _steeringTarget, Color.blue);
#endif

            //get the entity positions
            Vector3 ballPosition = Ball.Instance.NormalizedPosition;

            //set the look target
            Owner.RPGMovement.SetRotateFacePosition(ballPosition);

            //if I have exhausted my time then update the tend point
            if (_timeSinceLastUpdate <= 0f)
            {
                //do not continue if the ball didnt move
                if (_prevBallPosition != ballPosition)
                {
                    //cache the ball position
                    _prevBallPosition = ballPosition;

                    //run the logic for protecting the goal, find the position
                    Vector3 ballRelativePosToGoal = Owner.TeamGoal.GoalLineReference.InverseTransformPoint(ballPosition);
                    ballRelativePosToGoal.z = Owner.TendGoalDistance;
                    ballRelativePosToGoal.x /= Owner.TendGoalHorizontalMovemetInfluence;
                    ballRelativePosToGoal.x = Mathf.Clamp(ballRelativePosToGoal.x, 
                        Owner.TeamGoal.BottomLeftRelativePosition.x, 
                        Owner.TeamGoal.BottomRightRelativePosition.x);
                    
                    // transfrom relative ball position to world position
                    Vector3 ballWorldPosition = Owner.TeamGoal.GoalLineReference.transform.TransformPoint(ballRelativePosToGoal);
                    _steeringTarget = ballWorldPosition;

                    //add some noise to the target
                    float limit = 1f - Owner.GoalKeeping;
                    _steeringTarget.x += Random.Range(-limit, limit);
                    _steeringTarget.z += Random.Range(-limit, limit);
                }

                //reset the time 
                _timeSinceLastUpdate = 2f * (1f - Owner.GoalKeeping);
                if (_timeSinceLastUpdate == 0f)
                    _timeSinceLastUpdate = 2f * 0.1f;
            }

            //decrement the time
            _timeSinceLastUpdate -= Time.deltaTime;

            //set the ability to steer here
            Owner.RPGMovement.Steer = Owner.IsAtTarget(_steeringTarget) == false;
            Owner.RPGMovement.SetMoveTarget(_steeringTarget);

            // update forwards and turn if moving
            if (Owner.RPGMovement.Steer)
            {
                // get my relative velocity
                Vector3 relativeVelocity = Owner.transform.InverseTransformDirection(Owner.RPGMovement.MovementDirection);
                float clampedForward = Mathf.Clamp(relativeVelocity.z, -1f, 1f);
                float clampedSide = Mathf.Clamp(relativeVelocity.x, -1f, 1f);

                //update the animator
                Owner.Animator.SetFloat("Forward", clampedForward, 0.1f, 0.1f);
                Owner.Animator.SetFloat("Turn", clampedSide, 0.1f, 0.1f);
            }
            else
            {
                //update the animator
                Owner.Animator.SetFloat("Forward", 0f, 0.1f, 0.1f);
                Owner.Animator.SetFloat("Turn", 0f, 0.1f, 0.1f);
            }
        }

        public override void ManualExecute()
        {
            base.ManualExecute();

            // run logic depending on whether team is in control or not
            if (Owner.IsTeamInControl == true)
                SuperMachine.ChangeState<GoToHomeMainState>();
        }

        public override void Exit()
        {
            base.Exit();

            // set the animator
            Owner.Animator.ResetTrigger("TendGoal");

            //deregister to some events
            Owner.OnShotTaken -= Instance_OnShotTaken;
            Owner.OnInstructedToWait -= Instance_OnWait;
        }

        private void Instance_OnShotTaken(float flightTime, float velocity, Vector3 initial, Vector3 target)
        {
            // get the direction to target
            Vector3 direction = target - initial;
           
            // make a raycast and test if it hits a goal
            RaycastHit hitInfo;
            bool willBallHitAGoal = Physics.SphereCast(initial + Vector3.up * Ball.Instance.SphereCollider.radius,
                        Ball.Instance.SphereCollider.radius,
                        direction,
                        out hitInfo,
                        300,
                        _goalLayerMask);

            // get the goal from the goal trigger
            if (willBallHitAGoal)
            {
                // get the goal
                Goal goal = hitInfo.transform.GetComponent<GoalTrigger>().Goal;

                // check if shot is on target
                bool isShotOnTarget = goal == Owner.TeamGoal;

                if (isShotOnTarget == true)
                {
                    // init the intercept shot state
                    InterceptShotMainState interceptShotState = Machine.GetState<InterceptShotMainState>();
                    interceptShotState.BallInitialPosition = initial;
                    interceptShotState.BallInitialVelocity = velocity;
                    interceptShotState.ShotTarget = target;

                    // trigger state change
                    Machine.ChangeState<InterceptShotMainState>();
                }
            }
        }

        private void Instance_OnWait()
        {
            Machine.ChangeState<WaitMainState>();
        }

        public Player Owner
        {
            get
            {
                return ((GoalKeeperFSM)SuperMachine).Owner;
            }
        }
    }
}
