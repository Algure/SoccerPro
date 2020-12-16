using Assets.SimpleSteering.Scripts.Movement;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Utilities;
using Assets.FootballGameEngine_Indie.Scripts.Utilities.Enums;
using RobustFSM.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.InGame.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.References;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.InFieldPlayerStates.TacklePlayer.MainState;
using Assets.FootballGameEngine_Indie.Scripts.States.Entities.PlayerStates.InFieldPlayerStates.Tackled.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.Utilities.Enums;
using System.Linq;
using Assets.FootballGameEngine_Indie_.Scripts.UI.Widgets.InfoWidgets.PlayerInfoWidget;
using Assets.FootballGameEngine_Indie_.Scripts.Entities;
using System.Collections;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Assets.FootballGameEngine_Indie.Scripts.Entities
{
    [RequireComponent(typeof(RPGMovement))]
    [RequireComponent(typeof(SupportSpot))]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        bool _isUserControlled;

        [Header("AI Variables")]

        [Range(0.1f, 1f)]
        [SerializeField]
        float _tightPressFrequency = 0.5f;

        [SerializeField]
        float _ballContrallableDistance = 1f;

        [SerializeField]
        float _ballLongPassArriveVelocity = 8f;

        [SerializeField]
        float _ballShortPassArriveVelocity = 5f;

        [SerializeField]
        float _ballShotArriveVelocity = 10f;

        [SerializeField]
        float _ballTacklableDistance = 3f;

        [SerializeField]
        float _distanceMaxWonder = 10f;

        [SerializeField]
        float _distancePassMax = 15f;

        [SerializeField]
        float _distancePassMin = 5f;

        [SerializeField]
        float _distanceShotMaxValid = 20f;

        [SerializeField]
        float _distanceThreatMax = 0.5f;

        [SerializeField]
        float _distanceThreatMin = 1f;

        [SerializeField]
        float _rotationSpeed = 3.5f;

        [SerializeField]
        float _threatTrackDistance = 1f;

        [SerializeField]
        float _tendGoalHorizontalMovemetInfluence = 3f;

        [SerializeField]
        float _tendGoalDistance = 1f;

        [SerializeField]
        [Range(0.1f, 5f)]
        float _tendGoalSpeed = 4f;


        [Header("Basic Player Attributes")]

        [SerializeField]
        [Range(0.1f, 1f)]
        float _accuracy;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _goalKeeping = 0.8f;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _diveDistance;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _jumpHeight;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _power;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _speed;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _tackling;

        [SerializeField]
        PlayerTypes _playerType;


        [Header("Goal Keeper Attributes")]

        [SerializeField]
        [Range(0.1f, 1f)]
        float _diveSpeed;

        [SerializeField]
        [Range(0.1f, 1f)]
        float _reach;


        [Header("Tactic Variable")]
        [SerializeField]
        bool _canJoinCornerKick;

        [SerializeField]
        float _maxShotDistance = 20;//Shot power

        [Header("Player Components")]

        /// <summary>
        /// A reference to this instance's animator
        /// </summary>
       // [SerializeField]
        Animator _animator;

        [Header("Player Meshes")]

        [SerializeField]
        SkinnedMeshRenderer _glovesMesh;

        [SerializeField]
        SkinnedMeshRenderer _kitMesh;

        [SerializeField]
        Transform _modelRoot;

        [Header("Ball Positions")]
        [SerializeField]
        Transform _ballDropKickPosition;

        [SerializeField]
        Transform _ballFrontPosition;

        [SerializeField]
        Transform _ballTopPosition;

        [Header("Entities")]

        [SerializeField]
        BallReference _ballReference;

        [SerializeField]
        Goal _oppGoal;

        [SerializeField]
        Goal _teamGoal;

        [SerializeField]
        Transform _homeRegion;

        [SerializeField]
        List<Player> _oppositionMembers;

        [SerializeField]
        List<Player> _teamMembers;

        [SerializeField]
        List<SupportSpot> _pitchPoints;

        [SerializeField]
        PitchRegions _pitchRegion;

        [Header("Widgets")]

        [SerializeField]
        PlayerControlInfoWidget _playerControlInfoWidget;

        [SerializeField]
        PlayerDirectionInfoWidget _playerDirectionInfoWidget;

        [SerializeField]
        PlayerHealthInfoWidget _playerHealthInfoWidget;

        [SerializeField]
        PlayerNameInfoWidget _playerNameInfoWidget;


        float _radius;

        Player _prevPassReceiver;
        RPGMovement _rpgMovement;
        public VariableJoystick joyStick;
        public MatchManager MatchManager;
        Vector3 PlayerShotDirection;
        public bool ShouldStopMoving;
        public Transform DefaultPoint;
        Transform holdPosition;
        internal TeamChoice teamChoice;
        public float verticalOffset = 0;
        public float horizontalOffset = 0;
        public Team team;


        public Action OnBecameTheClosestPlayerToBall;
        public Action OnGoalKeeperGainedControlOfBall;
        public Action OnInstructedToDefendCornerKick;
        public Action OnInstructedToGoToHome;
        public Action OnInstructedToInteractWithBall;
        public Action OnInstructedToPutBallBackIntoPlay;
        public Action OnInstructedToSupportCornerKick;
        public Action OnInstructedToTakeCornerKick;
        public Action OnInstructedToTakeGoalKick;
        public Action OnInstructedToTakeKickOff;
        public Action OnInstructedToTakeThrowIn;
        public Action OnInstructedToWait;
        public Action OnIsNoLongerClosestPlayerToBall;
        public Action OnNoSupportSpotFound;
        public Action OnPunchBall;
        public Action OnPutBallBackIntoPlay;
        public Action OnTackled;
        public Action OnTakeGoalKick;
        public Action OnTakeKickOff;
        public Action OnTakeThrowIn;
        public Action OnTeamGainedPossession;
        public Action OnTeamLostControl;

        public Action<Player> OnTrackThreat;
        public Action<Player, SupportSpot> OnGoToSupportSpot;

        public delegate void ShotTaken(float flightPower, float velocity, Vector3 initial, Vector3 target);
        public delegate void ChaseBallDel(Player player);
        public delegate void ControlBallDel(Player player);
        public delegate void InstructedToReceiveBall(float time, Vector3 position);
        public delegate void TakeCornerKick(float ballTime, Vector3? position, Player receiver);

        public ShotTaken OnShotTaken;
        public ChaseBallDel OnChaseBall;
        public ControlBallDel OnControlBall;
        public TakeCornerKick OnTakeCornerKick;

        public InstructedToReceiveBall OnInstructedToReceiveBall;

        public bool HasBall { get; set; }

        [SerializeField]
        public bool IsTeamInControl;// { get; set; }

        public bool IsUserControlled { get => _isUserControlled; set => _isUserControlled = value; }

        public float ActualAccuracy { get; set; }
        public float ActualDiveSpeed { get; set; }
        public float ActuaDiveDistance { get; set; }
        public float ActualJogSpeed { get; set; }
        public float ActuaJumpHeight { get; set; }
        public float ActualPower { get; set; }
        public float ActualReach { get; set; }
        public float ActualSprintSpeed { get; set; }
        public float BallTime { get; set; }


        public float ForwardRunFrequency { get; set; }
        public float Height { get; set; }
        public float KickPower { get; set; }
        public float KickTime { get; set; }
        public float LongBallFrequency { get; set; }
        public float SprintAnimatorValue { get; set; }
        public float TightPressFrequency { get => _tightPressFrequency; set => _tightPressFrequency = value; }

        public Vector3? KickTarget { get; set; }

        public IFSM GoalKeeperFSM { get; set; }
        public IFSM InFieldPlayerFSM { get; set; }
        public KickDecisions KickDecision { get; set; }
        public PassTypesEnum PassType { get; set; }
        public MatchStatuses MatchStatus { get; set; }

        public Pass Pass { get; set; }
        public Player PassReceiver { get; set; }
        public Shot Shot { get; set; }
        public SupportSpot SupportSpot { get; set; }
        public Transform HomePosition { get => _homeRegion; set => _homeRegion = value; }

        public List<Player> OppositionMembers { get => _oppositionMembers; set => _oppositionMembers = value; }
        public List<Player> TeamMembers { get => _teamMembers; set => _teamMembers = value; }

        public bool isFrozen = false;
        #region MonoBehaviour Methods
        private void Awake()
        {
            //get some components
            GoalKeeperFSM = GetComponent<GoalKeeperFSM>();
            InFieldPlayerFSM = GetComponent<InFieldPlayerFSM>();
            RPGMovement = GetComponent<RPGMovement>();
            SupportSpot = GetComponent<SupportSpot>();

            // cache some component data
            _radius = GetComponent<CharacterController>().radius;
            Height = GetComponent<CharacterController>().height;

            if (_playerType == PlayerTypes.InFieldPlayer)
            {
                if (_glovesMesh != null)
                {
                    _glovesMesh.gameObject.SetActive(false);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ball")
                Ball.Instance.OwnerWithLastTouch = this;
        }

        #endregion

        public bool IsPlayerReadyToPlay()
        {
            if (PlayerType == PlayerTypes.Goalkeeper)
                return true;
            else
                return !InFieldPlayerFSM.IsCurrentState<TackleMainState>() && !InFieldPlayerFSM.IsCurrentState<TackledMainState>();
        }

        public bool CanBallReachPoint(Vector3 fromPosition, Vector3 toPosition, float power, out float time)
        {
            //calculate the time
            time = TimeToTarget(fromPosition,
                       toPosition,
                       power,
                       Ball.Instance.Friction);

            //return result
            return time > 0;
        }

        public bool CanBallReachPoint(Vector3 toPosition, float power, out float time)
        {
            //calculate the time
            CanBallReachPoint(Ball.Instance.NormalizedPosition,
                       toPosition,
                       power,
                       out time);

            //return result
            return time > 0;
        }

        public bool CanMakeThrowIn(bool limitFiledOfView, Vector3 direction, out Pass result)
        {
            // prepare some variables
            bool isThrowPossible = false;
            result = null;
            Dictionary<Player, List<Tuple<float, float, Vector3>>> playerAndPassOptions = new Dictionary<Player, List<Tuple<float, float, Vector3>>>();

            // loop though and get a player I can pass to
            foreach (Player player in TeamMembers)
            {
                // check some conditions
                bool isPlayerNotMe = player != this;
                bool isPlayerInFieldPlayer = player.PlayerType == PlayerTypes.InFieldPlayer;
                bool isPlayerInCorrectDirection = limitFiledOfView ? Vector3.Angle(direction, player.Position - Position) <= 30f : true;
                bool isPlayerReadyToPlay = player.IsPlayerReadyToPlay();
                bool isPlayerWithinThrowRange = Vector3.Distance(Position, player.Position) <= 30f;

                // continue if all the conditions are true
                if (isPlayerInCorrectDirection
                    && isPlayerInFieldPlayer
                    && isPlayerNotMe
                    && isPlayerReadyToPlay
                    && isPlayerWithinThrowRange)
                {
                    /* check if a pass is safe to that point */

                    //get the possible pass options
                    List<Tuple<float, float, Vector3>> possiblePlayerPasses = new List<Tuple<float, float, Vector3>>();
                    List<Vector3> passOptions = GetPassPositionOptions(player.Position);

                    //loop through each option and search if it is possible to 
                    //pass to it. Consider positions higher up the pitch
                    foreach (Vector3 passOption in passOptions)
                    {
                        //find power to kick ball
                        float power = FindPower(Ball.Instance.NormalizedPosition,
                        passOption,
                        _ballLongPassArriveVelocity,
                        0f);

                        //clamp the power to the player's max power
                        power = Mathf.Clamp(power, 0f, this.ActualPower);

                        // get time to target
                        float ballTimeToTarget = TimeToTarget(Ball.Instance.NormalizedPosition,
                           passOption,
                           power);

                        // get time of player to point
                        float timeOfReceiverToTarget = TimeToTarget(passOption,
                            passOption,
                            ActualSprintSpeed);

                        // check if receiver can reach point before the ball
                        bool canReceiverReacheBallBeforeTarget = timeOfReceiverToTarget < ballTimeToTarget;

                        // continue if receiver can reach point before ball
                        if (canReceiverReacheBallBeforeTarget == true)
                        {
                            // check if pass is safe
                            bool isThrowSafe = IsLongPassSafeFromAllOpponents(Position,
                                player.Position,
                                passOption,
                                power,
                                ballTimeToTarget);

                            // if throw is safe continue
                            if (isThrowSafe)
                            {
                                // add the pass option to the possible player pass
                                possiblePlayerPasses.Add(new Tuple<float, float, Vector3>(ballTimeToTarget, power, passOption));
                            }
                        }
                    }

                    // check if we have something in the player passes
                    bool isPlayerPassesAvailable = possiblePlayerPasses.Count > 0;

                    // if we have possible passes then add the player and his passes to the dictionary
                    if (isPlayerPassesAvailable)
                        playerAndPassOptions.Add(player, possiblePlayerPasses);
                }
            }

            // check if we have players to pass to
            bool isReceiversAvailable = playerAndPassOptions.Count > 0;

            // procedd if we have receivers 
            if (isReceiversAvailable)
            {
                // set throw is possible
                isThrowPossible = true;

                // randomly choose a player and a pass position
                int randomIndex = Random.Range(0, playerAndPassOptions.Count);
                KeyValuePair<Player, List<Tuple<float, float, Vector3>>> chosenPlayerAndPassOptions = playerAndPassOptions.ElementAt(randomIndex);

                // randomly choose a pass option
                randomIndex = Random.Range(0, chosenPlayerAndPassOptions.Value.Count);
                Tuple<float, float, Vector3> randomlyChoosenPassOption = chosenPlayerAndPassOptions.Value[randomIndex];

                // set the data
                Pass pass = new Pass()
                {
                    BallTimeToTarget = randomlyChoosenPassOption.Item1,
                    KickPower = randomlyChoosenPassOption.Item2,

                    PassType = PassTypesEnum.Long,

                    FromPosition = Position,
                    ToPosition = randomlyChoosenPassOption.Item3,

                    Receiver = chosenPlayerAndPassOptions.Key
                };

                // set the result
                result = pass;
            }

            // return result
            return isThrowPossible;
        }

        /// <summary>
        /// Checks whether a player can pass
        /// </summary>
        /// <returns></returns>
        /// ToDo::Implement logic to cache players to message so that they can intercept the pass
        public bool CanPass(out Pass result, bool considerPassSafety = true)
        {
            // cached passes
            List<Pass> optionalPasses = new List<Pass>();

            //loop through each team player and find a pass for each
            foreach (Player receiver in TeamMembers)
            {
                // can't pass to myself
                bool isPlayerMe = receiver == this;
                if (isPlayerMe)
                    continue;

                // we don't want to pass to the last receiver
                bool isPlayePrevPassReceiver = receiver == _prevPassReceiver;
                if (isPlayePrevPassReceiver)
                    continue;

                // can't pass to the goalie
                bool isPlayerGoalKeeper = receiver.PlayerType == PlayerTypes.Goalkeeper;
                if (isPlayerGoalKeeper)
                    continue;

                // we can't pass to a tackling or tackled player
                if (receiver.InFieldPlayerFSM.IsCurrentState<TackleMainState>() && receiver.InFieldPlayerFSM.IsCurrentState<TackledMainState>())
                    continue;

                // find the best pass for this player
                Pass newPass = FindBestPass(receiver, considerPassSafety);
                if(newPass != null)optionalPasses.Add(newPass);
            }

            // get the furthest pass up the field
            Pass bestPass = optionalPasses.
                OrderBy(oP => Vector3.Distance(oP.ToPosition, OppGoal.Position))
                .FirstOrDefault();

            // set the out pass
            result = bestPass;
           
            // return result
            return bestPass != null;
        }

        public Pass FindBestPass(Player receiver, bool considerPassSafety = true, bool considerPlayerClosestToMe = false)
        {
            // return result
            return FindBestPass(receiver.Position, receiver, considerPassSafety, considerPlayerClosestToMe);
        }

        public Pass FindBestPass(Vector3 toPosition, Player receiver, bool considerPassSafety = true, bool considerPlayerClosestToMe = false)
        {
            //get the possible pass options
            List<Vector3> passOptions = GetPassPositionOptions(toPosition);
            passOptions.Add(toPosition);

            // check if the player should make a long ball
            bool isLongBall = Random.value <= LongBallFrequency;

            //make the appropriate pass
            if (isLongBall)
            {
                // find the best short pass
                Pass bestLongPass = FindBestLongPass(Ball.Instance.Position, passOptions, receiver);

                // return result
                return bestLongPass;
            }
            else
            {
                // find the best short pass
                Pass bestShortPass = FindBestShortPass(Ball.Instance.Position, passOptions, receiver);

                // return result
                return bestShortPass;
            }
        }

        internal void MoveToDefaults()
        {
            _movingToDefaults = true;
        }

        public Pass FindBestLongPass(Vector3 fromPosition, List<Vector3> toPassOptions, Player receiver, bool considerPassSafety = true)
        {
            // set some default data
            bool isBestPassOptionFound = false;
            float distanceToGoal = 100f;
            Pass bestPassOption = new Pass();

            foreach (Vector3 toPassOption in toPassOptions)
            {
                // declare some variables
                float ballTimeToTarget = 0;
                float kickPower = 0;

                // check if canMakeShortPass
                bool canMakeShortPass = CanMakeLongPass(fromPosition, toPassOption, receiver, out ballTimeToTarget, out kickPower, considerPassSafety);
                if (canMakeShortPass == true)
                {
                    // choose a pass option higher up the pitch
                    float currDistanceToGoal = Vector3.Distance(fromPosition, toPassOption);
                    if (currDistanceToGoal < distanceToGoal)
                    {
                        // set is best pass option found to true
                        isBestPassOptionFound = true;

                        // cache the clossest distannce to goal so far
                        distanceToGoal = currDistanceToGoal;

                        // create the curr pass
                        bestPassOption.Init(ballTimeToTarget, kickPower, PassTypesEnum.Long, fromPosition, toPassOption, receiver);
                    }
                }
            }

            // if we have the best pass option return that one
            if (isBestPassOptionFound == true)
                return bestPassOption;
            else
                return null;
        }

        internal void StopMoveToDefaults()
        {
            _movingToDefaults = false;
        }

        public Pass FindBestShortPass(Vector3 fromPosition, List<Vector3> toPassOptions, Player receiver, bool considerPassSafety = true)
        {
            // set some default data
            bool isBestPassOptionFound = false;
            float distanceToGoal = 100f;
            Pass bestPassOption = new Pass();

            foreach(Vector3 toPassOption in toPassOptions)
            {
                // declare some variables
                float ballTimeToTarget = 0;
                float kickPower = 0;

                // check if canMakeShortPass
                bool canMakeShortPass = CanMakeShortPass(fromPosition, toPassOption, receiver, out ballTimeToTarget, out kickPower, considerPassSafety);
                if(canMakeShortPass == true)
                {
                    // choose a pass option higher up the pitch
                    float currDistanceToGoal = Vector3.Distance(fromPosition, toPassOption);
                    if(currDistanceToGoal < distanceToGoal)
                    {
                        // set is best pass option found to true
                        isBestPassOptionFound = true;

                        // cache the clossest distannce to goal so far
                        distanceToGoal = currDistanceToGoal;

                        // create the curr pass
                        bestPassOption.Init(ballTimeToTarget, kickPower, PassTypesEnum.Short, fromPosition, toPassOption, receiver);
                    }
                }
            }

            // if we have the best pass option return that one
            if (isBestPassOptionFound == true)
                return bestPassOption;
            else
                return null;
        }

        public bool CanMakeLongPass(Vector3 fromPosition, Vector3 toPosition, Player receiver, out float ballTimeToTarget, out float kickPower, bool considerPassSafety = true)
        {
            // assign out parameters
            ballTimeToTarget = 0;
            kickPower = 0;

            // check if position is within pass range
            bool isPositionWithinLongPassRange = IsPositionWithinLongPassRange(toPosition);

            // we consider a target which is out of our min pass distance
            if (isPositionWithinLongPassRange == true)
            {
                //find power to kick ball
                kickPower = FindPower(fromPosition,
                    toPosition,
                    _ballLongPassArriveVelocity,
                    0f);

                //clamp the power to the player's max power
                kickPower = Mathf.Clamp(kickPower, 0f, ActualPower);

                //find if ball can reach point
                bool canBallReachTarget = CanBallReachPoint(fromPosition,
                    toPosition,
                    kickPower,
                    out ballTimeToTarget);

                //return false if the time is less than zero
                //that means the ball can't reach it's target
                if (canBallReachTarget == false)
                    return false;

                // if we have a receiver, check if receiver can reach target before the ball
                if (receiver != null)
                {
                    // get time of receiver to point
                    float timeOfReceiverToTarget = TimeToTarget(receiver.Position,
                        toPosition,
                        ActualSprintSpeed);// use my speed, since I don't know the high speed of the receiver

                    // pass is not safe if receiver can't reach target before the ball
                    if (timeOfReceiverToTarget > ballTimeToTarget)
                        return false;
                }

                // check if pass is safe from all opponents
                bool isLongPassSafeFromAllOpponents = true;
                if (considerPassSafety)
                {
                    // check pass safety
                    isLongPassSafeFromAllOpponents = IsLongPassSafeFromAllOpponents(fromPosition,
                        receiver?.Position,
                        toPosition,
                        kickPower,
                        ballTimeToTarget);
                }

                // return result
                return isLongPassSafeFromAllOpponents;
            }

            // return false
            return false;
        }

        public bool CanMakeShortPass(Vector3 fromPosition, Vector3 toPosition, bool considerPassSafety = true, Player receiver = null)
        {
            // declare some variables
            float ballTimeToTarget;
            float kickPower;

            // return result
            return CanMakeShortPass(fromPosition, toPosition, receiver, out ballTimeToTarget, out kickPower, considerPassSafety);
        }

        public bool CanMakeShortPass(Vector3 fromPosition, Vector3 toPosition, Player receiver, out float ballTimeToTarget, out float kickPower, bool considerPassSafety = true)
        {
            // assign out parameters
            ballTimeToTarget = 0;
            kickPower = 0;

            // check if position is within pass range
            bool isPositionWithinShortPassRange = IsWithinDistanceRange(fromPosition, 
                toPosition, 
                _distancePassMin, 
                _distancePassMax); 

            // we consider a target which is out of our min pass distance
            if (isPositionWithinShortPassRange == true)
            {
                //find power to kick ball
                kickPower = FindPower(fromPosition,
                    toPosition,
                    _ballShortPassArriveVelocity,
                    Ball.Instance.Friction);

                //clamp the power to the player's max power
                kickPower = Mathf.Clamp(kickPower, 0f, ActualPower);

                //find if ball can reach point
                bool canBallReachTarget = CanBallReachPoint(fromPosition,
                    toPosition,
                    kickPower,
                    out ballTimeToTarget);

                //return false if the time is less than zero
                //that means the ball can't reach it's target
                if (canBallReachTarget == false)
                    return false;

                // if we have a receiver, check if receiver can reach target before the ball
                if (receiver != null)
                {
                    // get time of receiver to point
                    float timeOfReceiverToTarget = TimeToTarget(receiver.Position,
                        toPosition,
                        ActualSprintSpeed);// use my speed, since I don't know the high speed of the receiver

                    // pass is not safe if receiver can't reach target before the ball
                    if (timeOfReceiverToTarget > ballTimeToTarget)
                        return false;
                }

                // check if pass is safe from all opponents
                bool isPassSafeFromAllOpponents = true;
                if (considerPassSafety)
                {
                    // check pass safety
                    isPassSafeFromAllOpponents = IsShortBallKickSafeFromAllOpponents(fromPosition,
                        receiver?.Position,
                        toPosition,
                        kickPower);
                }

                // return result
                return isPassSafeFromAllOpponents;
            }

            // return false
            return false;
        }

        public bool CanMakeCornerKick(out Pass result)
        {
            // reset result
            result = null;

            // pick a random player inside the attack corner-kick region
            Player[] players = TeamMembers.Where(tM => 
                    tM != this 
                    && IsPositionWithinDistance(Position, tM.Position, 43f))
                .ToArray();

            // get a random player
            int randomIndex = Random.Range(0, players.Length - 1);
            Player player = players[randomIndex];

            //find power to kick ball
            float power = FindPower(Ball.Instance.NormalizedPosition,
                player.Position,
                _ballLongPassArriveVelocity,
                0f);

            // get time to target
            float ballTimeToTarget = TimeToTarget(Ball.Instance.NormalizedPosition,
               player.Position,
               power);

            // set other variables
            result = new Pass()
            {
                BallTimeToTarget = ballTimeToTarget,
                KickPower = power,

                PassType = PassTypesEnum.Long,

                FromPosition = Position,
                ToPosition = player.Position,

                Receiver = player
            };

            // return result
            return player != null;
        }

        public bool CanScore(out Shot shot, bool considerGoalDistance = true, bool considerShotSafety = true)
        {
            // set default value for shot
            shot = null;

            // shoot if distance to goal is valid
            if (considerGoalDistance)
            {
                bool isDistanceValid = IsPositionWithinDistance(OppGoal.Position, Position, _distanceShotMaxValid);
                if (isDistanceValid == false)
                    return false;
            }

            //define some positions to be local to the goal
            //get the shot reference point. It should be a point some distance behinde the 
            //goal-line/goal
            Vector3 refShotTarget = _oppGoal.ShotTargetReferencePoint;

            //number of tries to find a shot
            float numOfTries = Random.Range(1, 6);

            //loop through and find a valid shot
            for (int i = 0; i < numOfTries; i++)
            {
                //find a random target
                Vector3 randomGoalTarget = FindRandomShot();

                float power = FindPower(Ball.Instance.NormalizedPosition,
                    randomGoalTarget,
                    _ballShotArriveVelocity);

                //clamp the power
                power = Mathf.Clamp(power, 0f, ActualPower);
              
                //check if ball can reach the target
                float time = 0f;
                bool canBallReachPoint = CanBallReachPoint(randomGoalTarget,
                    power,
                    out time);

                // if ball can't reach target then return false
                if (canBallReachPoint == false)
                    return false;

                //check if shot to target is possible
                bool isShotPossible = true;
                //if (considerShotSafety)
                //{
                //    isShotPossible = IsShortBallKickSafeFromAllOpponents(Ball.Instance.NormalizedPosition,
                //        null,
                //        randomGoalTarget,
                //        power);
                //}

                //if shot is possible set the data
                if (isShotPossible == false && considerShotSafety == false
                    || isShotPossible == true && considerShotSafety == true)
                {
                    // recalculate the time
                    time = TimeToTarget(Ball.Instance.NormalizedPosition,
                        randomGoalTarget,
                        _ballShotArriveVelocity);

                    // calculate the y-target
                    float yPos = (power / _ballShotArriveVelocity) * (TeamGoal.GoalHeight) * _accuracy * 0.1f; // Random.value;
                    randomGoalTarget.y = yPos;

                    //set the data
                    shot = new Shot()
                    {
                        BallTimeToTarget = time,
                        KickPower = power,

                        ShotTypeEnum = ShotTypesEnum.Default,

                        ToPosition = randomGoalTarget
                    };

                    //return result
                    return true;
                }
            }

            return false;
        }

        //public bool CanPlayerReachTargetBeforePlayer(Vector3 target, Player player001, Player player002)
        //{
        //    return IsPositionCloserThanPosition(target,
        //        player001.Position,
        //        player002.Position);
        //}

        public bool CanLongPassInDirection(Vector3 direction, out Pass result)
        {
            //set the pass target
            result = null;

            // cached passes
            List<Pass> optionalPasses = new List<Pass>();
            bool passToPlayerClosestToMe = Random.value <= 0.75f;

            //loop through each team player and find a pass for each
            foreach (Player receiver in TeamMembers)
            {
                // find a pass to a player who isn't me
                // who isn't a goal keeper
                // who is in this direction
                if (receiver != this
                    && receiver.PlayerType == PlayerTypes.InFieldPlayer
                    && IsPositionInDirection(direction, receiver.Position, 60f))
                {
                    //get the possible pass options
                    List<Vector3> passOptions = GetPassPositionOptions(receiver.Position);
                    passOptions.Add(receiver.Position);

                    // find the best short pass
                    Pass bestLongPass = FindBestLongPass(Ball.Instance.Position, passOptions, receiver);
                    optionalPasses.Add(bestLongPass);
                }
            }

            // get the furthest pass up the field
            Pass bestPass = optionalPasses
                .OrderBy(oP => Vector3.Distance(oP.ToPosition, OppGoal.Position))
                .FirstOrDefault();

            // set the out pass
            result = bestPass;

            // if there is no pass simply find a team member in this direction
            if (result == null)
            {
                //loop through each team player and find a pass for each
                foreach (Player receiver in TeamMembers)
                {
                    // find a pass to a player who isn't me
                    // who isn't a goal keeper
                    // who is in this direction
                    if (receiver != this
                        && receiver.PlayerType == PlayerTypes.InFieldPlayer
                        && IsPositionInDirection(direction, receiver.Position, 22.5f))
                    {
                        //get the possible pass options
                        List<Vector3> passOptions = GetPassPositionOptions(receiver.Position);
                        passOptions.Add(receiver.Position);

                        // find the best short pass
                        Pass bestLongPass = FindBestLongPass(Ball.Instance.Position, passOptions, receiver, false);
                        optionalPasses.Add(bestLongPass);
                    }
                }
            }

            // get the furthest pass up the field
            bestPass = optionalPasses
                .OrderBy(oP => Vector3.Distance(oP.ToPosition, OppGoal.Position))
                .FirstOrDefault();

            // set the out pass
            result = bestPass;

            //return result
            return result != null;
        }

        public bool CanShortPassInDirection(Vector3 direction, out Pass result)
        {
            //set the pass target
            List<Pass> optionalPasses = new List<Pass>();
            bool passToPlayerClosestToMe = Random.value <= 0.75f;

            //set the pass target
            result = null;

            //loop through each team player and find a pass for each
            foreach (Player receiver in TeamMembers)
            {
                // find a pass to a player who isn't me
                // who isn't a goal keeper
                // who is in this direction
                if (receiver != this
                    && receiver.PlayerType == PlayerTypes.InFieldPlayer
                    && IsPositionInDirection(direction, receiver.Position, 60f))
                {
                    //get the possible pass options
                    List<Vector3> passOptions = GetPassPositionOptions(receiver.Position);
                    passOptions.Add(receiver.Position);

                    // find the best pass for this player
                    Pass newPass = FindBestShortPass(Ball.Instance.Position, passOptions, receiver);
                    if (newPass != null) optionalPasses.Add(newPass);
                }
            }

            // get the furthest pass up the field
            Pass bestPass = optionalPasses
                .OrderBy(oP => Vector3.Distance(oP.ToPosition, OppGoal.Position))
                .FirstOrDefault();

            // set the out pass
            result = bestPass;

            // if there is no pass simply find a team member in this direction
            if (result == null)
            {
                //loop through each team player and find a pass for each
                foreach (Player receiver in TeamMembers)
                {
                    // find a pass to a player who isn't me
                    // who isn't a goal keeper
                    // who is in this direction
                    if (receiver != this
                        && receiver.PlayerType == PlayerTypes.InFieldPlayer
                        && IsPositionInDirection(direction, receiver.Position, 22.5f))
                    {
                        //get the possible pass options
                        List<Vector3> passOptions = GetPassPositionOptions(receiver.Position);
                        passOptions.Add(receiver.Position);

                        // find the best pass for this player
                        Pass newPass = FindBestShortPass(Ball.Instance.Position, passOptions, receiver, false);
                        if (newPass != null) optionalPasses.Add(newPass);
                    }
                }
            }

            // get the furthest pass up the field
            bestPass = optionalPasses
                .OrderBy(oP => Vector3.Distance(oP.ToPosition, OppGoal.Position))
                .FirstOrDefault();

            // set the out pass
            result = bestPass;

            //return result
            return result != null;
        }

        public bool IsBallPositionThreateningGoal()
        {
            return IsPositionThreateningGoal(Ball.Instance.Position);
        }

        public bool IsPositionThreateningGoal(Vector3 position)
        {
            return IsPositionWithinDistance(_teamGoal.Position, position, 50f);
        }

        public bool IsLongPassSafeFromAllOpponents(Vector3 initialPosition, Vector3? receiverPosition, Vector3 target, float initialBallVelocity, float time)
        {
            //look for a player threatening the pass
            foreach (Player player in OppositionMembers)
            {
                bool isPassSafeFromOpponent = IsLongPassSafeFromOpponent(initialPosition,
                    target,
                    player.Position,
                    receiverPosition,
                    initialBallVelocity,
                    time);

                //return false if the pass is not safe
                if (isPassSafeFromOpponent == false)
                    return false;
            }

            //return result
            return true;
        }

        public bool IsLongPassSafeFromOpponent(Vector3 initialPosition, Vector3 target, Vector3 oppPosition, Vector3? receiverPosition, float initialBallVelocity, float timeOfBall)
        {
            #region Consider some logic that might threaten the pass

            //we might not want to pass to a player who is highly threatened(marked)
            if (IsPositionHighlyThreatened(target, oppPosition))
                return false;

            //return false if opposition is closer to target than reciever
            if (receiverPosition != null)
            {
                if (IsPositionCloserThanPosition(target, oppPosition, (Vector3)receiverPosition))
                    return false;
            }

            // return result
            return true;

            #endregion
        }

        public bool IsShortBallKickSafeFromAllOpponents(Vector3 fromPosition, Vector3? receiverPosition, Vector3 toPosition, float initialBallVelocity)
        {
            //if there's an opposition player threatening the pass, then return false
            foreach (Player opp in OppositionMembers)
            {
                bool isPassSafeFromOpponent = IsShortBallKickSafeFromOpponent(fromPosition,
                    toPosition,
                    opp.Position,
                    receiverPosition,
                    initialBallVelocity);

                //return false if the pass is not safe
                if (isPassSafeFromOpponent == false)
                    return false;
            }

            //return result
            return true;
        }

        public bool IsShortBallKickSafeFromOpponent(Vector3 fromPosition, Vector3 toPosition, Vector3 oppPosition, Vector3? receiverPosition, float initialBallVelocity)
        {
            #region Consider some logic that might threaten the pass

            //we might not want to pass to a player who is highly threatened(marked)
            if (IsPositionHighlyThreatened(toPosition, oppPosition))
                return false;

            if (receiverPosition != null)
            {
                //return false if opposition is closer to target than reciever
                if (IsPositionCloserThanPosition(toPosition, oppPosition, (Vector3)receiverPosition))
                    return false;

                //If oppossition is not between the passing lane then he is behind the passer
                //receiver and he can't intercept the ball
                if (IsPositionBetweenTwoPoints(fromPosition, (Vector3)receiverPosition, oppPosition) == false)
                    return true;
            }

            #endregion

            #region find if opponent can intercept ball

            //check if pass to position can be intercepted
            Vector3 orthogonalPoint = GetPointOrthogonalToLine(fromPosition,
                toPosition,
                oppPosition);

            //get time of ball to point
            float timeOfBallToOrthogonalPoint = 0f;
            CanBallReachPoint(orthogonalPoint, initialBallVelocity, out timeOfBallToOrthogonalPoint);

            // find the point the player can intercept ball
            float ballControllableDistance = _ballContrallableDistance + Radius;

            Vector3 oppDirToOrthogonalPoint = orthogonalPoint - oppPosition;
            if (oppDirToOrthogonalPoint.magnitude > ballControllableDistance)
                orthogonalPoint = oppPosition + oppDirToOrthogonalPoint.normalized * (oppDirToOrthogonalPoint.magnitude - ballControllableDistance);

            //get time of opponent to target
            float timeOfOpponentToTarget = TimeToTarget(oppPosition,
            orthogonalPoint,
            ActualSprintSpeed);

            //ball is safe if it can reach that point before the opponent
            bool canBallReachOrthogonalPointBeforeOpp = timeOfBallToOrthogonalPoint < timeOfOpponentToTarget;

            if (canBallReachOrthogonalPointBeforeOpp == true)
                return true;
            else
                return false;
            // return true;
            #endregion
        }

        /// <summary>
        /// Checks whether this instance is picked out or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsPickedOut(Player player)
        {
            return SupportSpot.IsPickedOut(player);
        }

        public bool IsPositionBetweenTwoPoints(Vector3 A, Vector3 B, Vector3 point)
        {
            //find some direction vectors
            Vector3 fromAToPoint = point - A;
            Vector3 fromBToPoint = point - B;
            Vector3 fromBToA = A - B;
            Vector3 fromAToB = -fromBToA;

            //check if point is inbetween and return result
            return Vector3.Dot(fromAToB.normalized, fromAToPoint.normalized) > 0
                && Vector3.Dot(fromBToA.normalized, fromBToPoint.normalized) > 0;
        }

        /// <summary>
        /// Checks whether the first position is closer to target than the second position
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position001"></param>
        /// <param name="position002"></param>
        /// <returns></returns>
        public bool IsPositionCloserThanPosition(Vector3 target, Vector3 position001, Vector3 position002)
        {
            return Vector3.Distance(position001, target) < Vector3.Distance(position002, target);
        }

        public bool IsPositionInDirection(Vector3 forward, Vector3 position, float angle)
        {
            // find direction to target
            Vector3 directionToTarget = position - Position;

            // find angle between forward and direction to target
            float angleBetweenDirections = Vector3.Angle(forward.normalized, directionToTarget.normalized);

            // return result
            return angleBetweenDirections <= angle / 2;
        }

        public bool IsPositionThreatened(Vector3 position)
        {
            return OppositionMembers.Any(oTM => IsPositionThreatened(position, oTM.Position));
        }

        public bool IsPositionWithinMinPassDistance(Vector3 position)
        {
            return IsPositionWithinDistance(Position,
                position,
                _distancePassMin);
        }

        public bool IsPositionWithinMinPassDistance(Vector3 center, Vector3 position)
        {
            return IsPositionWithinDistance(center,
                position,
                _distancePassMin);
        }

        public bool IsPositionWithinWanderRadius(Vector3 position)
        {
            return IsPositionWithinDistance(_homeRegion.position,
                position,
                _distanceMaxWonder);
        }

        /// <summary>
        /// Finds the power
        /// </summary>
        /// <param name="from">initial position</param>
        /// <param name="to">target</param>
        /// <param name="arriveVelocity">required velocity on arrival to target</param>
        /// <param name="friction">force acting against motion</param>
        /// <returns></returns>
        public float FindPower(Vector3 from, Vector3 to, float arriveVelocity, float friction)
        {
            // v^2 = u^2 + 2as => u^2 = v^2 - 2as => u = root(v^2 - 2as)

            //calculate some values
            float vSquared = Mathf.Pow(arriveVelocity, 2f);
            float twoAS = 2 * friction * Vector3.Distance(from, to);
            float uSquared = vSquared - twoAS;

            //find result
            float result = Mathf.Sqrt(uSquared);

            //return result
            return result;
        }

        public float TimeToTarget(Vector3 fromPosition, Vector3 toPosition, float velocityInitial)
        {
            //use S = D/T => T = D/S
            return Vector3.Distance(fromPosition, toPosition) / velocityInitial;
        }

        /// <summary>
        /// Calculates the time it will take to reach the target
        /// </summary>
        /// <param name="inital">start position</param>
        /// <param name="target">final position</param>
        /// <param name="initialVelocity">initial velocity</param>
        /// <param name="acceleration">force acting aginst motion</param>
        /// <returns></returns>
        public float TimeToTarget(Vector3 initial, Vector3 target, float velocityInitial, float acceleration)
        {
            //using  v^2 = u^2 + 2as 
            float distance = Vector3.Distance(initial, target);
            float uSquared = Mathf.Pow(velocityInitial, 2f);
            float v_squared = uSquared + (2 * acceleration * distance);

            //if v_squared is less thaSn or equal to zero it means we can't reach the target
            if (v_squared <= 0)
                return -1.0f;

            //find the final velocity
            float v = Mathf.Sqrt(v_squared);

            //find time to travel 
            return TimeToTravel(velocityInitial, v, acceleration);
        }

        public float TimeToTravel(float initialVelocity, float finalVelocity, float acceleration)
        {
            // t = v-u
            //     ---
            //      a
            float time = (finalVelocity - initialVelocity) / acceleration;

            //return result
            return time;
        }

        /// <summary>
        /// Finds a random target on the goal
        /// </summary>
        /// <returns></returns>
        public Vector3 FindRandomShot()
        {
            //define some positions to be local to the goal
            //get the shot reference point. It should be a point some distance behinde the 
            //goal-line/goal
            Vector3 refShotTarget = _oppGoal.transform.InverseTransformPoint(_oppGoal.ShotTargetReferencePoint);

            //find an x-position within the goal mouth
            float randomXPosition = Random.Range(_oppGoal.BottomLeftRelativePosition.x,
                _oppGoal.BottomRightRelativePosition.x);

            //set result
            Vector3 goalLocalTarget = new Vector3(randomXPosition, refShotTarget.y, refShotTarget.z);
            Vector3 goalGlobalTarget = _oppGoal.transform.TransformPoint(goalLocalTarget);

            //return result
            return goalGlobalTarget;
        }

        public Player GetClosestTeamPlayerToPosition(Vector3 position)
        {
            // get the player
            Player player = _teamMembers.Where(tM => tM != this && tM.IsPlayerReadyToPlay())
                .OrderBy(tM => Vector3.Distance(tM.Position, position))
                .First();

            // return result
            return player;
        }

        /// <summary>
        /// Calculates a point on line a-b that is at right angle to a point
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 GetPointOrthogonalToLine(Vector3 from, Vector3 to, Vector3 point)
        {
            //this is the normal
            Vector3 fromTo = to - from;

            //this is the vector/direction
            Vector3 fromPoint = point - from;

            //find projection
            Vector3 projection = Vector3.Project(fromPoint, fromTo);

            //find point on normal
            return projection + from;
        }

        /// <summary>
        /// Gets the options to pass the ball
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public List<Vector3> GetPassPositionOptions(Vector3 position)
        {
            //create a list to hold the results
            List<Vector3> result = new List<Vector3>();

            //the first position is the current position
            result.Add(position);

            //set some data
            float incrementAngle = 45;
            float iterations = 360 / incrementAngle;

            //find some positions around the player
            for (int i = 0; i < iterations; i++)
            {
                //get the direction
                float angle = incrementAngle * i;

                //rotate the direction
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

                //get point
                Vector3 point = position + direction * _distancePassMin * Random.value;

                //add to list
                result.Add(point);
            }

            //return results
            return result;
        }

        /// <summary>
        /// Initializes this instance
        /// </summary>
        public void Init()
        {
            ActuaDiveDistance *= _diveDistance;
            ActualDiveSpeed *= _diveSpeed;
            ActuaJumpHeight *= _jumpHeight;
            ActualPower *= _power;
            ActualReach *= _reach;
            ActualRotationSpeed =  _rotationSpeed * _speed;
            ActualSprintSpeed += ActualJogSpeed;

            //Init the RPGMovement
            RPGMovement.Init(_speed, 
                ActualJogSpeed,
                ActualRotationSpeed, 
                ActualSprintSpeed);
        }

        /// <summary>
        /// Initializes this instance
        /// </summary>
        /// <param name="power"></param>
        /// <param name="speed"></param>
        public void Init(float aiUpdateFrequency, 
           float distancePassMax,
           float distancePassMin,
           float distanceShotValidMax,
           float distanceTendGoal,
           float distanceThreatMax,
           float distanceThreatMin,
           float distanceThreatTrack,
           float distanceWonderMax,
           float velocityLongPassArrive,
           float velocityShortPassArrive,
           float velocityShotArrive,
           float diveDistance,
           float diveSpeed,
           float jogSpeed,
           float jumpHeight,
           float forwardRunFrequency,
           float longBallFrequency,
           float tightPressFrequency,
           float power,
           float reach,
           float speed,
           PitchRegions pitchRegion,
           Texture kit,
           InGamePlayerDto playerDto)
        {
            // set up the fsm
            if (GoalKeeperFSM != null)
            {
                GoalKeeperFSM.ManualUpdateFrequency = aiUpdateFrequency;
                GoalKeeperFSM.StartManualExecute();
            }

            if (InFieldPlayerFSM != null)
            {
                InFieldPlayerFSM.ManualUpdateFrequency = aiUpdateFrequency;
                InFieldPlayerFSM.StartManualExecute();
            }

            // ai values
            _distancePassMax = distancePassMax;
            _distancePassMin = distancePassMin;
            _distanceShotMaxValid = distanceShotValidMax;
            _tendGoalDistance = distanceTendGoal;
            _threatTrackDistance = distanceThreatTrack;
            _distanceMaxWonder = distanceWonderMax;
            _ballLongPassArriveVelocity = velocityLongPassArrive;
            _ballShortPassArriveVelocity = velocityShortPassArrive;
            _ballShotArriveVelocity = velocityShotArrive;
            _distanceThreatMax = distanceThreatMax;
            _distanceThreatMin = distanceThreatMin;

            // set the actual values
            ActuaDiveDistance = diveDistance;
            ActualDiveSpeed = diveSpeed;
            ActualJogSpeed = jogSpeed;
            ActuaJumpHeight = jumpHeight;
            ActualPower = power;
            ActualReach = reach;
            ActualSprintSpeed = (speed - jogSpeed) * playerDto.Speed;

            ForwardRunFrequency = forwardRunFrequency;
            LongBallFrequency = longBallFrequency;
            TightPressFrequency = tightPressFrequency;

            // setup the player attributes
            _accuracy = playerDto.Accuracy;
            _canJoinCornerKick = playerDto.CanJoinCornerKick;
            _diveDistance = playerDto.DiveDistance;
            _diveSpeed = playerDto.DiveSpeed;
            _goalKeeping = playerDto.GoalKeeping;
            _jumpHeight = playerDto.JumpHeight;
            _power = playerDto.Power;
            _reach = playerDto.Reach;
            _speed = playerDto.Speed;
            _tackling = playerDto.Tackling;

            // set the mesh texture
            _kitMesh.material.mainTexture = kit;

            // init pitc values
            _pitchRegion = pitchRegion;

            // calculate the sprint animator value
            SprintAnimatorValue = 0.5f + (0.5f * ((speed - jogSpeed) / ActualSprintSpeed));

            // init the info component
            string displayName = string.Format("{0}. {1}", playerDto.KitNumber, playerDto.KitName);
            _playerNameInfoWidget.Init(displayName);
            _playerNameInfoWidget.Root.SetActive(false);
        }

        public bool IsAheadOfMe(Vector3 position)
        {
            // find the relative positons
            Vector3 playerRelativePositionToGoal = _teamGoal.transform.InverseTransformPoint(Position);
            Vector3 positionRelativePositionToGoal = _teamGoal.transform.InverseTransformPoint(position);

            // return result
            return playerRelativePositionToGoal.z <= positionRelativePositionToGoal.z;
        }

        /// <summary>
        /// Checks whether the player is at target
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsAtTarget(Vector3 position)
        {
            return IsPositionWithinDistance(Position, position, 0.5f);
        }

        public bool IsBallContrallable()
        {
            return IsBallWithinControllableDistance() && IsBallWithinControllableHeight();
        }

        public bool IsBallWithinControllableDistance()
        {
            return IsPositionWithinDistance(Position, Ball.Instance.NormalizedPosition, _ballContrallableDistance + Radius);
        }

        public bool IsBallWithinControllableHeight()
        {
            return Ball.Instance.Position.y <= Height;
        }

        public bool IsBallTacklable()
        {
            return IsBallWithinTacklableDistance() && IsBallWithinControllableHeight();
        }

        public bool IsBallWithinTacklableDistance()
        {
            return IsPositionWithinDistance(Position, Ball.Instance.NormalizedPosition, _ballTacklableDistance + _radius);
        }

        public bool IsInfrontOfPlayer(Vector3 position)
        {
            // find the direction to target
            Vector3 direction = position - Position;

            // find the dot product
            float dot = Vector3.Dot(direction.normalized, transform.forward);

            return dot > 0.5;
        }

        /// <summary>
        /// Checks whether a player is a threat
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsPlayerAThreat(Player player)
        {
            return IsPositionWithinDistance(Position, player.Position, _distanceThreatMax);
        }

        public bool IsPositionThreatened(Vector3 center, Vector3 position)
        {
            return IsPositionWithinDistance(center, position, _distanceThreatMin);
        }

        public bool IsPositionWithinLongPassRange(Vector3 position)
        {
            return IsPositionWithinDistance(Position,
                position,
                _distancePassMax) == false;
        }

        public bool IsPositionWithinShortPassRange(Vector3 position)
        {
            return IsWithinDistanceRange(Position,
                position,
                _distancePassMin,
                _distancePassMax);
        }

        public bool IsPositionWithinShortPassRange(Vector3 center, Vector3 position)
        {
            return IsWithinDistanceRange(center,
                position,
                _distancePassMin,
                _distancePassMax);
        }

        /// <summary>
        /// Check whether a position is a threat or not
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsPositionAThreat(Vector3 position)
        {
            // position is a threat if its within saftey distance
            return IsPositionWithinDistance(Position, position, DistanceThreatMax);
        }

        public bool IsPositionAThreat(Vector3 center, Vector3 position)
        {
            return IsPositionWithinDistance(center, position, DistanceThreatMax);
        }

        public bool IsPositionHighlyThreatened(Vector3 center, Vector3 position)
        {
            return IsPositionWithinDistance(center, position, _distanceThreatMax);
        }

        public bool IsTeamMemberWithinMinPassDistance(Vector3 position)
        {
            return _teamMembers.ToList()
                .Any(tM => tM != this && IsPositionWithinDistance(position, tM.Position, _distancePassMin));
        }

        public bool IsTeamMemberWithinDistance(float distance, Vector3 position)
        {
            return _teamMembers.ToList()
                .Any(tM => tM != this && IsPositionWithinDistance(tM.Position, position, distance));
        }

        public bool IsThreatened()
        {
            // return true if there is any threatening player
            return OppositionMembers
                .Any(oP => IsPlayerAThreat(oP) == true);
        }

        public bool IsPositionWithinDistance(Vector3 center, Vector3 position, float distance)
        {
            return Vector3.Distance(center, position) <= distance;
        }

        public bool IsWithinDistanceRange(Vector3 center, Vector3 position, float minDistance, float maxDistance)
        {
            return !IsPositionWithinDistance(center, position, minDistance) && IsPositionWithinDistance(center, position, maxDistance);
        }

        /// <summary>
        /// Finds the power needed to kick the ball and make it reach
        /// a particular target with a particular velocity
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="arrivalVelocity"></param>
        /// <returns></returns>
        public float FindPower(Vector3 from, Vector3 to, float arrivalVelocity)
        {
            //find the power to target
            float power = Ball.Instance.FindPower(from,
                to,
                arrivalVelocity);

            //return result
            return power;
        }

        #region invoke actions region

        public void Invoke_OnShotTaken(float flightTime, float velocity, Vector3 initial, Vector3 target)
        {
            ShotTaken temp = OnShotTaken;
            if (temp != null)
                temp.Invoke(flightTime, velocity, initial, target);
        }

        public void Invoke_OnControlBall()
        {
            // raise event that I'm controlling the ball
            ControlBallDel temp = OnControlBall;
            if (temp != null)
                temp.Invoke(this);
        }

        public void Invoke_OnBecameTheClosestPlayerToBall()
        {
            ActionUtility.Invoke_Action(OnBecameTheClosestPlayerToBall);
        }

        public void Invoke_OnInstructedToDefendCornerKick()
        {
            ActionUtility.Invoke_Action(OnInstructedToDefendCornerKick);
        }

        public void Invoke_OnInstructedGoToHome()
        {
            ActionUtility.Invoke_Action(OnInstructedToGoToHome);
        }

        public void Invoke_OnInstructedToInteractWithBall()
        {
            ActionUtility.Invoke_Action(OnInstructedToInteractWithBall);
        }

        public void Invoke_OnInstructedToPutBallBackIntoPlay()
        {
            ActionUtility.Invoke_Action(OnInstructedToPutBallBackIntoPlay);
        }

        public void Invoke_OnInstructedToSupportCornerKick()
        {
            ActionUtility.Invoke_Action(OnInstructedToSupportCornerKick);
        }

        public void Invoke_OnInstructedToTakeKickOff()
        {
            ActionUtility.Invoke_Action(OnInstructedToTakeKickOff);
        }

        public void Invoke_OnInstructedToTakeCornerKick()
        {
            ActionUtility.Invoke_Action(OnInstructedToTakeCornerKick);
        }

        public void Invoke_OnInstructedToTakeGoalKick()
        {
            ActionUtility.Invoke_Action(OnInstructedToTakeGoalKick);
        }

        public void Invoke_OnInstructedToTakeThrowIn()
        {
            ActionUtility.Invoke_Action(OnInstructedToTakeThrowIn);
        }

        public void Invoke_OnInstructedToWait()
        {
            ActionUtility.Invoke_Action(OnInstructedToWait);
        }

        public void Invoke_OnIsNoLongerTheClosestPlayerToBall()
        {
            ActionUtility.Invoke_Action(OnIsNoLongerClosestPlayerToBall);
        }

        public void Invoke_OnPunchBall()
        {
            ActionUtility.Invoke_Action(OnPunchBall);
        }

        public void Invoke_OnTeamGainedPossession()
        {
            // set that my team is in control
            IsTeamInControl = true;

            // raise event that team is now in control
            ActionUtility.Invoke_Action(OnTeamGainedPossession);
        }

        public void Invoke_OnTeamLostControl()
        {
            // set team no longer in control
            IsTeamInControl = false;

            // invoke team has lost control
            ActionUtility.Invoke_Action(OnTeamLostControl);
        }
        #endregion

        /// <summary>
        /// Player kicks the ball from his position to the target
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void MakePass(Vector3 from, Vector3 to, Player receiver, float power, float time)
        {
            transform.LookAt(to);
            MoveWithBall();
            // kinck
           // if (Pass.PassType == PassTypesEnum.Long)
                Ball.Instance.LaunchToPoint(to, power);
            //  else if (Pass.PassType == PassTypesEnum.Short)
            //    Ball.Instance.KickToPoint(to, power);

            //message the receiver to receive the ball
            //  InstructedToReceiveBall temp = receiver.OnInstructedToReceiveBall;
            // if (temp != null)
            //   temp.Invoke(time, to);
            freezePlayer(Statics.freezeTimeAfterShot);
        }

        public void MakePass(Pass pass)
        {
            try
            {
                freezePlayer(Statics.freezeTimeAfterShot);

                // kick
                if (pass.PassType == PassTypesEnum.Long)
                    Ball.Instance.LaunchToPoint(pass.ToPosition, pass.KickPower);
                else if (pass.PassType == PassTypesEnum.Short)
                 Ball.Instance.KickToPoint(pass.ToPosition, pass.KickPower);
        
                //message the receiver to receive the ball
                InstructedToReceiveBall temp = pass.Receiver.OnInstructedToReceiveBall;
                if (temp != null)
                    temp.Invoke(pass.BallTimeToTarget, pass.ToPosition);
            }catch(Exception e)
            {
                print("exception: "+e.ToString());
            }
        }

        public void MakeShot(Shot shot)
        {
            freezePlayer(Statics.freezeTimeAfterShot);
            //launch the ball
            Ball.Instance.LaunchToPoint(shot.ToPosition, shot.KickPower);

            // raise the ball shot event
            Ball.BallLaunched temp = Ball.Instance.OnBallShot;
            if (temp != null)
                temp.Invoke(shot.BallTimeToTarget, shot.KickPower, shot.FromPosition, shot.ToPosition);
        }

        public void MakeShot(Vector3 from, Vector3 to, float power, float time)
        {
            freezePlayer(Statics.freezeTimeAfterShot);

            //launch the ball
            Ball.Instance.LaunchToPoint(to, power);

            // raise the ball shot event
            Ball.BallLaunched temp = Ball.Instance.OnBallShot;
            if (temp != null)
                temp.Invoke(time, power, from, to);
        }

        /// <summary>
        /// Puts the ball infront of this player
        /// </summary>
        public void PlaceBallInfronOfMe()
        {
            Ball.Instance.NormalizedPosition = Position + transform.forward * (_radius + _ballContrallableDistance);
            Ball.Instance.transform.rotation = transform.rotation;
        }

        public List<Player> GetTeamMembersInRadius(float radius)
        {
            //get the players
            List<Player> result = _teamMembers.FindAll(tM => Vector3.Distance(this.Position, tM.Position) <= radius 
            && this != tM);

            //retur result
            return result;
        }

        public Player GetRandomTeamMemberInRadius(float radius)
        {
            //get the list
            List<Player> players = GetTeamMembersInRadius(radius);

            //return random player
            if (players == null)
                return null;
            else
                return players[Random.Range(0, players.Count)];
        }

        /// <summary>
        /// Gets the infield player closest to the position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Player GetTeamMemberClosestToPosition(Vector3 position)
        {
            // get the closest player to point
            Player player = _teamMembers
                .Where(tM => tM != this && tM.PlayerType == PlayerTypes.InFieldPlayer)
                .OrderBy(tM => Vector3.Distance(tM.Position, position))
                .FirstOrDefault();

            // return result
            return player;
        }

        public Quaternion Rotation
        {
            get
            {
                return transform.rotation;
            }

            set
            {
                transform.rotation = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return new Vector3(transform.position.x, 0f, transform.position.z);
            }

            set
            {
                transform.position = new Vector3(value.x, 0f, value.z);
            }
        }

        public Goal OppGoal { get => _oppGoal; set => _oppGoal = value; }
        public float BallShortPassArriveVelocity { get => _ballShortPassArriveVelocity; set => _ballShortPassArriveVelocity = value; }
        public List<SupportSpot> PlayerSupportSpots { get => _pitchPoints; set => _pitchPoints = value; }

        public float DistancePassMin
        {
            get => _distancePassMin;
            set => _distancePassMin = value;
        }

        public float DistanceThreatMin
        {
            get => _distanceThreatMin + _radius;
            set => _distanceThreatMin = value;
        }

        public float DistanceThreatMax
        {
            get => _distanceThreatMax + _radius;
            set => _distanceThreatMax = value;
        }

        public RPGMovement RPGMovement
        {
            get
            {
                // set the rpg movement
                if (_rpgMovement == null)
                {
                    gameObject.AddComponent<RPGMovement>();
                    _rpgMovement = GetComponent<RPGMovement>();
                }

                // return result
                return _rpgMovement;
            }

            set
            {
                _rpgMovement = value;
            }
        }
        int dex = 0;
        private bool _movingToDefaults;
        private Transform keeperXPos;
        private Transform keeperXNeg;
        private Transform frontDropKickPosition;

        void Start()
        {
            
            isFrozen = false;
            _animator = this.transform.Find("Character").gameObject.GetComponent<Animator>();
            // OppGoal = GameObject.Find("SoccerStadium").transform.FindChild("Goal").GetComponent<Goal>();
           if(DefaultPoint!=null) this.Position = DefaultPoint.position;
        }

        // Update is called once per frame
        public void Update()
        {
            team.reportPlayerPosition(this);
            if (PlayerType==PlayerTypes.Goalkeeper && holdPosition==null)
                holdPosition = this.transform.Find("BallPositions").Find("BallFrontPosition");
            if (holdPosition == null)
            {

           // holdPosition = this.transform.Find("BallPositions").transform.Find("BallDropKickPosition");
            }
        }

        void FixedUpdate()
        {

             if (_animator == null)
             {
                 _animator = this.transform.Find("Character").gameObject.GetComponent<Animator>();
             }
             if (isFrozen)
             {
                 if (Ball.Instance.CurrentOwner == this) MoveWithBall();
                 _animator.Play("idle");
                 return;
             }

           /*  if (MatchManager.UserControlledPlayer != this && _movingToDefaults)
             {
                 RunToDefaultPoint();
                 return;
             }

             if (PlayerType != PlayerTypes.Goalkeeper)
             {
                 InFieldPlayerUpdate();
             }
             else
             {
                 GoalKeeperUpdate();
             }*/

        }

        private void GoalKeeperUpdate()
        {
            if (IsUserControlled)
            {
               // MatchManager.UserControlledPlayer = this;//TO BE OPTIMISED-LET THIS BE DONE BY TEAM SCRIPT
                MoveGoalKeeperWithControls();

            }
            else if (!IsUserControlled && !IsBallContrallable())
            {
                RunGoalKeeperTowardsBall();
            }
            else if (!IsUserControlled && IsBallTacklable() && Ball.Instance.CurrentOwner != this)
            {
                SaveBall();
            }
            else if (!IsUserControlled && IsBallContrallable() && Ball.Instance.CurrentOwner == this)
            {
                MoveWithBall();
                //Broadcast return all players to default point
                MakeGoalkeeperShootOrPass();
            }
        }

        private void MakeGoalkeeperShootOrPass()
        {
            //int select= int.Parse((Random.RandomRange(0.1f, 0.2f)).ToString());
            //Make A RANDOM SELECTION BETWEEN SHOOT OR PASS MakePass()
            //makeRandShot2Point(_oppGoal.transform.position);
            int ran1=Convert.ToInt32((Random.Range(0.1f, 0.2f))*10);
            if (ran1 == 1)
            {
                makeShotInForwardDirection(0.5f);
                MatchManager.SetDebugText("Made goalee shot");
            }
            else
            {
                makeRandCross();
            }
        }

        private void RunGoalKeeperTowardsBall()
        {
            if(keeperXPos==null) keeperXPos = _teamGoal.transform.Find("KeeperXPos");
            if(keeperXNeg==null) keeperXNeg = _teamGoal.transform.Find("KeeperXNeg");
            Vector3 ballPosition = Ball.Instance.Position;
            Vector3 nextPosition = ballPosition;
            nextPosition.y = DefaultPoint.position.y;
            nextPosition.z = DefaultPoint.position.z;

            if (transform.position.x == ballPosition.x|| 
                (ballPosition.x > keeperXPos.position.x && transform.position.x== keeperXPos.position.x) ||
                (ballPosition.x < keeperXNeg.position.x && transform.position.x== keeperXNeg.position.x)
               )
            {
                _animator.Play("idle");
                return;
            }
           // nextPosition.x = ballPosition.x;
            if (nextPosition.x > keeperXPos.position.x)
            {
                nextPosition.x = keeperXPos.position.x;
            }
            if (nextPosition.x < keeperXNeg.position.x)
            {
                nextPosition.x = keeperXNeg.position.x;
            }

            float step = Speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(this.Position, nextPosition, step);
            transform.LookAt(ballPosition);
            _animator.Play("sideStep");

            //ADD GOALKEEPER MOVEMENT BORDERS
            //ADD GOALKEEPER DEFAULT POSITION.

            // Clip x position if x exceeds 18 borders.
            // Translate keeper to detected position and play run side-ways animation. 
            // Always look at ball position.
            // Play Idle animation if former and current positions are equal.
        }

        private void InFieldPlayerUpdate()
        {

            /* if (_movingToDefaults != null && _movingToDefaults)
             {
                 RunToDefaultPoint();
                 return;
             }

             if (IsUserControlled)
             {
                //MatchManager.UserControlledPlayer = this;//TO BE OPTIMISED
                 MoveWithControls();
             }
             else if (!IsUserControlled && !IsBallContrallable())
             {
                 if (_playerType == PlayerTypes.Defender && BallIsInPlayerRange())
                 {
                     if (IsBallDefenderChasable() )
                     {
                         Player ballOwner = Ball.Instance.CurrentOwner;
                         if (ballOwner==null || ballOwner.teamChoice != teamChoice)
                             RunTowardsBall();
                         else
                             MoveToGoalPost();
                     }
                     else if(BallIsInPlayerRange())
                     {
                         RunToDefaultPoint();
                     }
                 }
                 else if(BallIsInPlayerRange())
                 {
                     Player ballOwner = Ball.Instance.CurrentOwner;
                     if (ballOwner == null || ballOwner.teamChoice != teamChoice)
                         RunTowardsBall();
                     else
                         MoveToGoalPost();
                }else {
                    MoveToGoalPost();

                 }
             }

             else if (!IsUserControlled && IsBallTacklable() && Ball.Instance.CurrentOwner != this)
             {
                 TackleOwner();
             }
             else if (!IsUserControlled && IsBallContrallable() && Ball.Instance.CurrentOwner == this)
             {
                 MoveToGoalPost();
             }*/
        }

        private bool BallIsInPlayerRange()
        {
            return (Math.Abs(Ball.Instance.transform.position.x - DefaultPoint.position.x) <= Statics.maxPlayerPursueRange);
        }

        private void RunToDefaultPoint()
        {
            Transform goalPost = DefaultPoint;

            Vector3 postPosition = goalPost.position;
            float step = Statics.GeneralSpeedFactor* Speed * Time.deltaTime;
            float magn = (postPosition - this.Position).magnitude;

            if (transform.position == goalPost.position)
            {
                this.transform.LookAt(_oppGoal.transform.position);
                if (Ball.Instance.CurrentOwner == this) MoveWithBall();
                _animator.Play("idle");
                return;
            }
            if (PlayerType != PlayerTypes.Goalkeeper)
            {
                _animator.Play("running");
            }
            else
            {
                _animator.Play("run");
            }
            transform.position = Vector3.MoveTowards(this.Position, postPosition, step);
            transform.LookAt(goalPost);
            if (Ball.Instance.CurrentOwner == this) MoveWithBall();
        }

        private void MoveToGoalPost()
        {
            Transform goalPost = OppGoal.transform;
            if (Ball.Instance.CurrentOwner != this)
            {
                if(PlayerType==PlayerTypes.InFieldPlayer && 110-Math.Abs(Position.z- team.goalKeeperPoint.position.z) <= Statics.moveToPostMaxDistance4Midfielder)
                {
                    print("mid fielder: " + ((110 - Math.Abs(Position.z - team.goalKeeperPoint.position.z)).ToString()));
                    _animator.Play("idle");
                    return;

                }
                else if(PlayerType==PlayerTypes.Striker && 110-Math.Abs(Position.z - team.goalKeeperPoint.position.z) <= Statics.moveToPostMaxDistance4Striker)
                {
                   print("Striker: "+((110 - Math.Abs(Position.z - team.goalKeeperPoint.position.z)).ToString()));
                    _animator.Play("idle");
                    return;
                }
            }
            bool madeScoreShot = tryToScore(goalPost);//Ball.Instance.CurrentOwner==this?tryToScore(goalPost):false;
            if (!madeScoreShot)
            {
                _animator.Play("running");
                Vector3 postPosition = goalPost.position;
                float step = Statics.GeneralSpeedFactor * Speed * Time.deltaTime;
                moveFromOffSet(step);
               // float magn = (postPosition - this.Position).magnitude;
                //print("distance to post:"+magn.ToString());           
                transform.position = Vector3.MoveTowards(this.Position, postPosition, step);
                transform.LookAt(goalPost);
                MoveWithBall();
            }
            else
            {
                freezePlayer();
            }
        }

        void moveFromOffSet(float step)
        {
            if (verticalOffset != 0)
            {
                MatchManager.SetDebugText("Vertical offset detected");
                transform.position += transform.forward * verticalOffset; //Vector3.MoveTowards(this.Position, transform.forward * verticalOffset, step);
                verticalOffset = 0;
            }
            if (horizontalOffset != 0)
            {
                MatchManager.SetDebugText("horizontal offset detected");
                transform.position += transform.right * horizontalOffset;//Vector3.MoveTowards(this.Position, transform.right * horizontalOffset, step);
                horizontalOffset = 0;
            }
        }
        
        bool tryToScore(Transform goalPost)
        {
            float rayDistance = 30;
            RaycastHit hit;
            print("trying to score");
            Transform goalPointsPack = goalPost.FindChild("Points");
            Transform goalPoint1 = goalPointsPack.FindChild("Point001");
            Transform goalPoint2 = goalPointsPack.FindChild("Point002");
            Transform goalPoint3 = goalPointsPack.FindChild("Point003");
            Transform goalPoint4 = goalPointsPack.FindChild("Point004");
            
             Vector3 dir = transform.position - goalPoint1.position;
             Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(transform.position, goalPoint1.position, out hit, rayDistance))
            {
                GameObject hitOb=hit.collider.gameObject;
                String name = hitOb.name.Trim();
                print("hit object: " + name);
                if (name== "Point001")
                {
                    print("got To MakeShot " + name);
                    makeRandShot2Point(goalPoint1.position);
                    return true;
                }
            }
              dir = transform.position - goalPoint2.position;
              ray = new Ray(transform.position, dir);
            if (Physics.Raycast(transform.position, goalPoint2.position, out hit, rayDistance))
            {
                GameObject hitOb=hit.collider.gameObject;
                String name = hitOb.name.Trim();
                print("hit object: " + name);
                if (name== "GoalTrigger")
                {
                    print("got To MakeShot " + name);
                    makeRandShot2Point(goalPoint2.position);
                    return true;
                }
            }
             dir = transform.position - goalPoint3.position;
             ray = new Ray(transform.position, dir);
            if (Physics.Raycast(transform.position, goalPoint3.position, out hit, rayDistance))
            {
                GameObject hitOb=hit.collider.gameObject;
                String name = hitOb.name.Trim();
                print("hit object: " + name);
                if (name== "GoalTrigger")
                {
                    print("got To MakeShot " + name);
                    makeRandShot2Point(goalPoint3.position);
                    return true;
                }
            }
             dir = transform.position - goalPoint4.position;
             ray = new Ray(transform.position, dir);
            if (Physics.Raycast(transform.position, goalPoint4.position, out hit, rayDistance))
            {
                GameObject hitOb=hit.collider.gameObject;
                String name = hitOb.name.Trim();
                if (name== "GoalTrigger")
                {
                    makeRandShot2Point(goalPoint4.position);
                    return true;
                }
            }

             dir = transform.position - goalPost.position;
             ray = new Ray(transform.position, dir);
            if (Physics.Raycast(transform.position, goalPost.position, out hit, rayDistance))
            {
                GameObject hitOb = hit.collider.gameObject;
                String name = hitOb.name.Trim();
                if (name == "GoalTrigger")
                {
                    makeRandShot();
                    return true;
                }
            }
            if (dir.magnitude <= Statics.GoalShotMinDistance)
            {
                int shotType = Convert.ToInt32(Random.Range(0.1f, 0.5f)*10);
                if (shotType == 1)
                {
                    makeRandShot2Point(goalPoint1.position);
                }else if (shotType == 2)
                {
                    print("got To MakeShot 2");
                    makeRandShot2Point(goalPoint2.position);
                }else if (shotType == 3)
                {
                    makeRandShot2Point(goalPoint3.position);
                }else if (shotType == 4)
                {
                    makeRandShot2Point(goalPoint4.position);
                }else if (shotType == 5)
                {
                    print("got To MakeShot pass" );
                    makeRandCross();
                }
                return true;
            }
            return false;
        }

         bool IsBallDefenderChasable()
            {
                return (teamChoice==TeamChoice.TeamAway? Ball.Instance.Position.z >= 0: Ball.Instance.Position.z<=0);
            }

        public void celebrateGoal()
        {

        }

        public void PutPlayerAtDefault(Transform defaultTransform)
        {
            DefaultPoint = defaultTransform;
            this.Position = DefaultPoint.position;
        }

        public void PutPlayerAtKickOff(Transform defaultTransform)
        {
            restoreDefaultPoint(DefaultPoint);
            DefaultPoint = defaultTransform;
            this.Position = DefaultPoint.position;
            
        }

        private void restoreDefaultPoint(Transform defaultPoint)
        {
            StartCoroutine(WaitAndRestoreDefaultPoint(defaultPoint));
        }

        private IEnumerator WaitAndRestoreDefaultPoint(Transform defaultPoint)
        {
            yield return new WaitForSeconds(Statics.TimeToRestoreDefaultPoint);
            DefaultPoint = defaultPoint;
        }

        private void RunTowardsBall()
        {
            _animator.Play("running");
            Vector3 ballPosition =Ball.Instance.transform.position;
            Vector3 moveFromPosition = this.Position;
            float step = Statics.GeneralSpeedFactor * Speed * Time.deltaTime;
            moveFromOffSet(step);
            transform.position = Vector3.MoveTowards(this.Position, ballPosition, step);
            transform.LookAt(Ball.Instance.transform);
        }

        private void MoveWithBall()
        {
            if (Ball.Instance.CurrentOwner != this)
            {
                return;
            }
            if (_playerType != PlayerTypes.Goalkeeper)
            {
                PlaceBallInfronOfMe();
            }
            else
            {
                HoldBall();
                //if(frontDropKickPosition== null) frontDropKickPosition = transform.Find("BallDropKickPosition");
                // Vector3 ballPos = Position + transform.forward * (_radius + _ballContrallableDistance);
                //ballPos.y = Ball.Instance.Position.y;
                //Ball.Instance.Position = ballPos;
                //Ball.Instance.transform.rotation = transform.rotation;
            }
            /*
             Ball.Instance.OwnerWithLastTouch = this;
              float offSet = 0.2f;
              Vector3 oldBallPosition= Ball.Instance.transform.position;
              Vector3 newBallPosition = transform.position+transform.forward*offSet;
              newBallPosition.y = transform.position.y;
              Ball.Instance.transform.position = newBallPosition;*/
        }

   
        void MoveGoalKeeperWithControls()
        {
            var camera = Camera.main;
            var forward = camera.transform.forward;
            var right = camera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            Vector3 lookPosition = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical);
            Vector3 direction = right * joyStick.Horizontal + forward * joyStick.Vertical;
           /* if (direction.magnitude != 0)
            {
                transform.LookAt(transform.position + direction);
                PlayerShotDirection = transform.position;
                transform.position += transform.forward * Time.deltaTime * Speed * 4;
                ////TEST BLOCK
                /* float step = Speed * Time.deltaTime;
                 float magn = (direction - this.Position).magnitude;
                 transform.position = Vector3.MoveTowards(this.Position, direction, step);
                ////TEST BLOCK
                _animator.Play("run");
                if (IsBallContrallable() && Ball.Instance.CurrentOwner == this) MoveWithBall();
                return;
            }*/
            if (direction.magnitude != 0)
            {
                if (IsBallTacklable() && Ball.Instance.CurrentOwner == this)
                {
                    //MatchManager.SetDebugText("Is owner of ball");
                    PlaceBallInfronOfMe();
                }
                else if (IsBallContrallable() && Ball.Instance.CurrentOwner != this)
                {
                    //   MatchManager.SetDebugText("Is NOT owner of ball");
                    TackleOwner();
                }
                else
                {
                    // MatchManager.SetDebugText("Null condition");
                }
                transform.LookAt(transform.position + direction);
                PlayerShotDirection = transform.position;
                transform.position += transform.forward * Time.deltaTime * Speed * 4;

                _animator.Play("run");
                return;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(Vector3.forward * Speed * Time.deltaTime);
                _animator.Play("run");
                if (IsBallContrallable()) MoveWithBall();
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(Vector3.left * Speed * Time.deltaTime);
                _animator.Play("run");
                if (IsBallContrallable()) MoveWithBall();
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(Vector3.back * Speed * Time.deltaTime);
                _animator.Play("run");
                if (IsBallContrallable()) MoveWithBall();
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(Vector3.right * Speed * Time.deltaTime);
                _animator.Play("run");
                if (IsBallContrallable()) MoveWithBall();
            }
            else if (!IsBallContrallable())
            {
               // _animator.Play("idle");
                //RunTowardsBall();
            }
            else if (IsBallTacklable() && Ball.Instance.CurrentOwner != this)
            {
                _animator.Play("idle");
                SaveBall();
            }
            else if (IsBallContrallable() && Ball.Instance.CurrentOwner == this)
            {
                _animator.Play("idle");
                MoveWithBall();
            }
        }
        public void SetPlayerKit(Material material)
        {
            Transform character = transform.Find("Character");
            character.Find("kit").GetComponent<Renderer>().material = material;
            character.Find("socks").GetComponent<Renderer>().material = material;
        }
        public void SaveBall()
        {
            //_animator.Play("catchBall");
            Ball.Instance.stopMovement();
            Ball.Instance.CurrentOwner = this;
            MoveWithBall();
            MatchManager.BroadcastMoveToDefaults();
           // freezePlayer();
          //  if (MatchManager.UserControlledPlayer == this)
          //  {
                StartCoroutine(WaitAndControlGoalee());
          //  }         
        }

        private IEnumerator WaitAndControlGoalee()
        {
            MoveWithBall();
            yield return new WaitForSeconds(Statics.TimeWaitAfterGoalSaved);
            MatchManager.BroadcastReturnFromDefaults();
            freezePlayer();
        }
     
        private void HoldBall()
        {
           // _animator.Play("holdBall");
          
            MatchManager.ball.transform.position =Position + transform.forward * (_radius + _ballContrallableDistance);
            MatchManager.ball.transform.rotation = transform.rotation;
        }
        
        private void HoldBallWithoutAnim()
        {
            if (holdPosition == null)
            {
                holdPosition = this.transform.Find("BallPositions").transform.Find("BallDropKickPosition");
            }
            MatchManager.ball.transform.position = holdPosition.position;//Position + transform.forward * (_radius + _ballContrallableDistance);
            MatchManager.ball.transform.rotation = transform.rotation;
        }

        void MoveWithControls()
        {
           /* if (IsBallTacklable() && Ball.Instance.CurrentOwner == this)
            {
                MatchManager.SetDebugText("Is owner of ball");
            }
            else if (IsBallContrallable() && Ball.Instance.CurrentOwner != this)
            {
                MatchManager.SetDebugText("Is NOT owner of ball");
       
            }
            else
            {
                MatchManager.SetDebugText("Null condition");
            }*/
            var camera = Camera.main;
            var forward = camera.transform.forward;
            var right = camera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            Vector3 lookPosition = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical);
            Vector3 direction = right * joyStick.Horizontal + forward * joyStick.Vertical;
            if (direction.magnitude != 0)
             {
                if (IsBallTacklable() && Ball.Instance.CurrentOwner == this)
                {
                    //MatchManager.SetDebugText("Is owner of ball");
                    // PlaceBallInfronOfMe();
                   // MatchManager.SetDebugText("move with ball");
                    MoveWithBall();
                }
                else if (IsBallContrallable() && Ball.Instance.CurrentOwner != this)
                {
                    //   MatchManager.SetDebugText("Is NOT owner of ball");
                  //  MatchManager.SetDebugText("TACKLE");
                    TackleOwner(true);
                }
                else
                {
                   // MatchManager.SetDebugText("Null condition");
                }
                transform.LookAt(transform.position + direction);
                PlayerShotDirection = transform.position;
                transform.position += transform.forward * Time.deltaTime * Speed*4;

                _animator.Play("running");
               
                return;
            }
            else if (!IsBallContrallable())
            {
                RunTowardsBall();
            }
            else if (IsBallContrallable() && Ball.Instance.CurrentOwner != this)
            {
                TackleOwner(true);
            }
            else if (IsBallTacklable() && Ball.Instance.CurrentOwner == this)
            {
                _animator.Play("running");
                transform.position += transform.forward * Time.deltaTime * Speed * 4;
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
               // MatchManager.SetDebugText("Ball is trackable");
               MoveWithBall();

                //     if (IsBallContrallable() && Ball.Instance.CurrentOwner==this) MoveWithBall();
            }
            else
            {
                //MatchManager.SetDebugText("Ball is not trackable");
                if (IsBallContrallable()) MoveWithBall();
                _animator.Play("running");
                transform.position += transform.forward * Time.deltaTime * Speed * 4;
            }
        }
        /* if (Input.GetKey(KeyCode.UpArrow))
         {
             transform.Translate(Vector3.forward * Speed * Time.deltaTime);
             _animator.Play("running");
             if (IsBallContrallable()) MoveWithBall();
         }
         else if (Input.GetKey(KeyCode.LeftArrow))
         {
             transform.Translate(Vector3.left * Speed * Time.deltaTime);
            _animator.Play("running");
             if (IsBallContrallable()) MoveWithBall();
         }
         else if (Input.GetKey(KeyCode.DownArrow))
         {
             transform.Translate(Vector3.back * Speed * Time.deltaTime);
            _animator.Play("runningBack");
             if (IsBallContrallable()) MoveWithBall();
         }
         else if (Input.GetKey(KeyCode.RightArrow))
         {
             transform.Translate(Vector3.right * Speed* Time.deltaTime);
             _animator.Play("running");
             if (IsBallContrallable()) MoveWithBall();
         }*/


        public void TackleOwner(bool forgetTeam = false)
        {
            Player formerOwner=Ball.Instance.CurrentOwner;
            if (formerOwner == this||(formerOwner!=null && formerOwner.team==this.team && !forgetTeam))
            {
                //MatchManager.SetDebugText("Owner NOT tackled");
                return;
            }
            //MatchManager.SetDebugText("Owner tackled");
            _animator.Play("tackle");
            if (formerOwner==null || (formerOwner!=null && formerOwner.Tackling <= this.Tackling))
            {
               // MatchManager.SetDebugText("Owner tackled");
                Ball.Instance.CurrentOwner = this;
                if(formerOwner!=null)formerOwner.freezePlayer();
                Ball.Instance.stopMovement();
                MoveWithBall();
            }
            else
            {
                //if(formerOwner==null)
              //  MatchManager.SetDebugText("CANT TACKLE:"+ formerOwner.Tackling.ToString()+",this:"+this.Tackling.ToString());
            }
        }

        public void makeRandPass()
        {
            if (!IsBallContrallable() || Ball.Instance.CurrentOwner != this) return;
            Pass pass = new Pass();
            pass.BallTimeToTarget = 0.1f;
            pass.KickPower = Power;
            pass.PassType = PassTypesEnum.Short;
            pass.FromPosition = this.Position;
            pass.ToPosition = team.getClosestPlayerToPost(this).transform.position;//new Vector3(OppGoal.Position.x, Ball.Instance.Position.y, OppGoal.Position.z);
            _animator.Play("shortPass");
            MakePass(pass);
            freezePlayer();
        }

        public void makeRandCross()
        {
            if (!IsBallContrallable() || Ball.Instance.CurrentOwner != this) return;
            /* Pass pass = new Pass();
            pass.BallTimeToTarget = 0.1f;
            pass.KickPower = Power;
            pass.PassType = PassTypesEnum.Long;
            pass.FromPosition = this.Position;
            pass.ToPosition =team.getClosestPlayerToPost(this).transform.position;
            new Vector3(OppGoal.Position.x, Ball.Instance.Position.y, OppGoal.Position.z);*/
            Player passRecever= team.getClosestPlayerToPost(this);
            _animator.Play("longPass");
            if (passRecever == null || this == passRecever)
                MatchManager.SetDebugText("Same receiver");
            else
            {
                MatchManager.SetDebugText("Pass made");
                MakePass(this.Position, passRecever.Position, passRecever, Power, 0.1f);
               // freezePlayer();
            }
        }

        public void makeRandShot()
        {
            if (!IsBallContrallable() || Ball.Instance.CurrentOwner != this) return;
            Shot shot = new Shot();
            shot.BallTimeToTarget = 0.1f;
            shot.KickPower = Power;
            shot.ShotTypeEnum= ShotTypesEnum.Default;
            shot.FromPosition = this.Position;
            shot.ToPosition = OppGoal.Position;
            _animator.Play("shootBall");
            MakeShot(shot);
            freezePlayer();
        }

        public void makeRandShot2Point(Vector3 position)
        {
            if (!IsBallContrallable() || Ball.Instance.CurrentOwner != this) return;
            Shot shot = new Shot();
            shot.BallTimeToTarget = 0.1f;
            shot.KickPower = Power;
            shot.ShotTypeEnum = ShotTypesEnum.Default;
            shot.FromPosition = this.Position;
            shot.ToPosition = position;
            _animator.Play("shootBall");
            MakeShot(shot);
            freezePlayer();
        }

        internal void makeShotInForwardDirection( float powerFact=1)
        {
            float shotDistance = _maxShotDistance*powerFact;
            if (!IsBallContrallable() || Ball.Instance.CurrentOwner != this) return;
           // Vector3 forward = transform.TransformDirection(transform.forward) * shotDistance;
          //  Debug.DrawLine(transform.position, forward, Color.green, 2, false);
            Shot shot = new Shot();
            shot.BallTimeToTarget = 0.1f;
            shot.KickPower = Power;// * powerFact;
            shot.ShotTypeEnum = ShotTypesEnum.Default;
            Vector3 position = Position+ transform.forward * shotDistance;
            position.y = 0;
            shot.FromPosition = transform.position;
            shot.ToPosition = position;
            Debug.DrawLine(transform.position, position, Color.blue, 2, false);
            if (PlayerType == PlayerTypes.Goalkeeper)
                _animator.Play("dropKick");
            else
                _animator.Play("shootBall");
            MakeShot(shot);
           // freezePlayer();
        }

        public void freezePlayer(float freezeTime=0.4f)
        {
            StartCoroutine(FreezeRoutine(freezeTime));
        }

        private IEnumerator FreezeRoutine(float secs)
        {
            isFrozen = true;
            yield return new WaitForSeconds(secs);
            isFrozen = false;
        }

        internal void TryToPassBall()
        {
            if(Ball.Instance.CurrentOwner==this && !IsUserControlled)
            makeRandCross();
        }

        public struct PlayerUpdateJob : IJobParallelForTransform
        {
            private bool isFrozen;
           // private PlayerUpdateJob BallCurrentOwner;

            public void Execute(int index, TransformAccess transform)
            {
                /*if (isFrozen)
                {
                    if (BallCurrentOwner == this) MoveWithBall();
                    _animator.Play("idle");
                    return;
                }

                if (MatchManager.UserControlledPlayer != this && _movingToDefaults)
                {
                    RunToDefaultPoint();
                    return;
                }

                if (PlayerType != PlayerTypes.Goalkeeper)
                {
                    InFieldPlayerUpdate();
                }
                else
                {
                    GoalKeeperUpdate();
                }*/
            }

            void MainUpdate()
            {
               /* if (player.isFrozen)
                {
                    if (Ball.Instance.CurrentOwner == this) MoveWithBall();
                    _animator.Play("idle");
                    return;
                }

               // if (MatchManager.UserControlledPlayer != this && _movingToDefaults)
                {
                    RunToDefaultPoint();
                    return;
                }

                if (PlayerType != PlayerTypes.Goalkeeper)
                {
                    InFieldPlayerUpdate();
                }
                else
                {
                    GoalKeeperUpdate();
                }
                */
            }
        }


        public float Radius { get => _radius; set => _radius = value; }
        public Goal TeamGoal { get => _teamGoal; set => _teamGoal = value; }
        public PlayerTypes PlayerType { get => _playerType; set => _playerType = value; }
        public float ThreatTrackDistance { get => _threatTrackDistance; set => _threatTrackDistance = value; }
        public float TendGoalSpeed { get => _tendGoalSpeed; set => _tendGoalSpeed = value; }
        public float TendGoalDistance { get => _tendGoalDistance; set => _tendGoalDistance = value; }
        public float GoalKeeping { get => _goalKeeping; set => _goalKeeping = value; }
        public float DistancePassMax { get => _distancePassMax; set => _distancePassMax = value; }
        public Player PrevPassReceiver { get => _prevPassReceiver; set => _prevPassReceiver = value; }
        public PlayerControlInfoWidget PlayerControlInfoWidget { get => _playerControlInfoWidget; set => _playerControlInfoWidget = value; }
        public SkinnedMeshRenderer KitMesh { get => _kitMesh; set => _kitMesh = value; }
        public float Tackling { get => _tackling; set => _tackling = value; }
        public Animator Animator { get => _animator; set => _animator = value; }
        public float Reach { get => _reach; set => _reach = value; }
        public float JumpDistance { get => _diveDistance; set => _diveDistance = value; }
        public float JumpHeight { get => _jumpHeight; set => _jumpHeight = value; }
        public Transform ModelRoot { get => _modelRoot; set => _modelRoot = value; }
        public float DiveSpeed { get => _diveSpeed; set => _diveSpeed = value; }
        public BallReference BallReference { get => _ballReference; set => _ballReference = value; }
        public Transform BallFrontPosition { get => _ballFrontPosition; set => _ballFrontPosition = value; }
        public Transform BallDropKickPosition { get => _ballDropKickPosition; set => _ballDropKickPosition = value; }
        public float TendGoalHorizontalMovemetInfluence { get => _tendGoalHorizontalMovemetInfluence; set => _tendGoalHorizontalMovemetInfluence = value; }
        public float BallContrallableDistance { get => _ballContrallableDistance; set => _ballContrallableDistance = value; }
        public float Speed { get => _speed; set => _speed = value; }
        public float ActualRotationSpeed { get; set; }
        public float BallLongPassArriveVelocity { get => _ballLongPassArriveVelocity; set => _ballLongPassArriveVelocity = value; }
        public Transform BallTopPosition { get => _ballTopPosition; set => _ballTopPosition = value; }
        public bool CanJoinCornerKick { get => _canJoinCornerKick; set => _canJoinCornerKick = value; }
        public float DistanceShotMaxValid { get => _distanceShotMaxValid; set => _distanceShotMaxValid = value; }
        public float DistanceMaxWonder { get => _distanceMaxWonder; set => _distanceMaxWonder = value; }
        public float DistanceMaxWonder1 { get => _distanceMaxWonder; set => _distanceMaxWonder = value; }
        public SkinnedMeshRenderer GlovesMesh { get => _glovesMesh; set => _glovesMesh = value; }
        public PitchRegions PitchRegions { get => _pitchRegion; set => _pitchRegion = value; }
        public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }
        public float Accuracy { get => _accuracy; set => _accuracy = value; }
        public float DiveDistance { get => _diveDistance; set => _diveDistance = value; }
        public float Power { get => _power; set => _power = value; }
        public PlayerNameInfoWidget PlayerNameInfoWidget { get => _playerNameInfoWidget; set => _playerNameInfoWidget = value; }
        public PlayerDirectionInfoWidget PlayerDirectionInfoWidget { get => _playerDirectionInfoWidget; set => _playerDirectionInfoWidget = value; }
        public PlayerHealthInfoWidget PlayerHealthInfoWidget { get => _playerHealthInfoWidget; set => _playerHealthInfoWidget = value; }
    }
}

public class TeamPlayerManager : MonoBehaviour
{
    List<Player> playerList = new List<Player>();

    private void Update()
    {
     //   var job = new PlayerUpdateJob();
       // var jobHandle=job.Schedule(playerList.Count, 1);
        //jobHandle.Complete();
    }
}

