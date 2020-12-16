using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.StateMachines.Managers;
using Assets.FootballGameEngine_Indie.Scripts.Utilities;
using Assets.FootballGameEngine_Indie.Scripts.Utilities.Enums;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.InGame.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.Settings;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.Storage.MatchDifficulties;
using Assets.FootballGameEngine_Indie_.Scripts.Triggers;
using Patterns.Singleton;
using RobustFSM.Interfaces;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.FootballGameEngine_Indie.Scripts.Managers
{
    [RequireComponent(typeof(MatchManagerFSM))]
    public class MatchManager : Singleton<MatchManager>
    {
        [SerializeField]
        MatchDifficultyTeamParam _cpuTeamParams;

        [SerializeField]
        MatchDifficultyTeamParam _userTeamParams;

        [Header("Entities")]

        [SerializeField]
        public Ball ball;

        [SerializeField]
        GameObject mainPlayer;

        [SerializeField]
        GameObject goalKeeperPlayer;
        
        [SerializeField]
        GameObject awayMainPlayer;

        [SerializeField]
        GameObject awayGoalKeeperPlayer;

        [SerializeField]
        ThrowInTrigger _leftThrowInTrigger;

        [SerializeField]
        ThrowInTrigger _rightThrowInTrigger;

        [SerializeField]
        Slider _shotSlider;

        [SerializeField]
        Team _teamAway;

        [SerializeField]
        Team _teamHome;

        [SerializeField]
        Transform _rootTeam;

        [SerializeField]
        Transform _transformCentreSpot;

        [SerializeField]
        Goal _goalPostHome;

        [SerializeField]
        Goal _goalPostAway;

        [SerializeField]
        Text _goalText;

        [SerializeField]
        Text AlertText;

        [SerializeField]
        Text DebugText;

        [SerializeField]
        VariableJoystick joyStick;

        [Header("Teams Data")]

        [SerializeField]
        InGameTeamDto _awayTeamData;

        [SerializeField]
        Text _homeScoreText;

        [SerializeField]
        Text _awayScoreText;

        [SerializeField]
        InGameTeamDto _homeTeamData;

        [SerializeField]
        Transform GoalHomePosition;

        [SerializeField]
        Transform GoalAwayPosition;

        [SerializeField]
        Transform[] _homeDefendPoints;

        [SerializeField]
        Transform[] _homeAttackPoints;

        [SerializeField]
        Transform[] _homeMidPoints;

        [SerializeField]
        Transform[] _awayDefendPoints;

        [SerializeField]
        Transform[] _awayAttackPoints;

        [SerializeField]
        Transform[] _awayMidPoints;

        [SerializeField]
        Transform kickOff_1;

        [SerializeField]
        Transform kickOff_2;

        [SerializeField]
        Transform kickOffBall;  

        public Player UserControlledPlayer;

        int _homeScore = 0;
        int _awayScore = 0;
        public bool returnToDefaults;
        public bool GameHalted;
        private GameTimeEnum _currGameTime;
        
        public GameTimeEnum CurrGameTime { get => _currGameTime; set => _currGameTime = value; }

        /// <summary>
        /// A reference to how long each half length is in actual time(m)
        /// </summary>
        public float ActualHalfLength { get; set; } = 1f;

        /// <summary>
        /// A reference to the normal half length
        /// </summary>
        public float NormalTimeHalfLength { get; set; } = 45;

        /// <summary>
        /// A reference to the next time that we have to stop the game
        /// </summary>
        public float NextStopTime { get; set; }

        /// <summary>
        /// A reference to the extra-time half length
        /// </summary>
        public float ExtraTimeHalfLength { get; set; } = 15;

        /// <summary>
        /// A reference to the current game half in play
        /// </summary>
        public int CurrentHalf { get; set; }

        /// <summary>
        /// Property to get or set this instance's fsm
        /// </summary>
        public IFSM FSM { get; set; }

        /// <summary>
        /// A reference to the match status of this instance
        /// </summary>
        public MatchStatuses MatchStatus { get; set; }

        public Vector3 CachedBallPosition { get; set; }


        public Action OnBroadcastTakeCornerKick;
        public Action OnBroadcastTakeKickOff;
        public Action OnBroadcastTakeGoalKick;
        public Action OnContinueToNormalTime;
        public Action OnContinueToSecondHalf;

        public Action OnEnterExhaustHalfState;
        public Action OnEnterWaitForKickToComplete;
        public Action OnEnterWaitForMatchOnInstruction;

        public Action OnExitHalfTimeState;
        public Action OnExitMatchOverState;
        public Action OnExitMatchPausedState;
        public Action OnExitWaitForKickToComplete;
        public Action OnExitWaitForMatchOnInstruction;
        public Action OnExitExhaustHalfState;
        public Action OnExitNormalTimeIsOverState;
        public Action OnExitMatchPaused;

        public Action OnFinishBroadcastMessage;
        public Action OnInitialize;
        public Action OnMatchPaused;
        public Action OnMesssagedToSwitchToMatchOn;
        public Action OnStopMatch;

        public Action<string> OnEnterHalfTimeState;
        public Action<string> OnEnterMatchOverState;
        public Action<string> OnEnterMatchPausedState;
        public Action<string> OnGoalScored;
        public Action<string> OnStartBroadcastMessage;
        public Action<string> OnEnterNormalTimeIsOverState;

        public Action<int, int, int> OnTick;
        public Action<Vector3> OnBroadcastTakeThrowIn;

        public override void Awake()
        {
            base.Awake();

            FSM = GetComponent<MatchManagerFSM>();
        }

        public void Init(MatchDifficultyTeamParam cpuMatchDifficultyParams, MatchDifficultyTeamParam userMatchDifficultyParams, MatchSettingsDto matchSettings, InGameTeamDto awayTeamData, InGameTeamDto homeTeamData)
        {
            _cpuTeamParams = cpuMatchDifficultyParams;
            _userTeamParams = userMatchDifficultyParams;

            _awayTeamData = awayTeamData;
            _homeTeamData = homeTeamData;

            _currGameTime = GameTimeEnum.NormalTime;

            ActualHalfLength = matchSettings.HalfLength;
        }

        public void Invoke_OnContinueToNormalTime()
        {
            ActionUtility.Invoke_Action(OnContinueToNormalTime);
        }

        public void Instance_OnContinueToSecondHalf()
        {
            ActionUtility.Invoke_Action(OnContinueToSecondHalf);
        }

        public void Instance_ExitMatchPaused()
        {
            ActionUtility.Invoke_Action(OnExitMatchPaused);
        }

        public void Instance_OnMatchPlayPaused()
        {
            ActionUtility.Invoke_Action(OnMatchPaused);
        }

        /// <summary>
        /// Raises the event that this instance has been messaged to switch to match on
        /// </summary>
        public void Instance_OnMessagedSwitchToMatchOn()
        {
            ActionUtility.Invoke_Action(OnMesssagedToSwitchToMatchOn);
        }

        public void Invoke_OnInitialize()
        {
            ActionUtility.Invoke_Action(OnInitialize);
        }

        void Start()
        { 
            returnToDefaults=false;
            Ball.Instance.CurrentOwner = null;
            //SetTestPlayersAttributes();
            SetPlayersAttributes();
        }

        private void SetTestPlayersAttributes()
        {
            _teamAway.goalPostPosition = _goalPostAway.Position;
            _teamAway.oppPostPosition = _goalPostHome.Position;
            _teamAway._midFieldPoints = _awayMidPoints;
            _teamAway._defenderFieldPoints = _awayDefendPoints;
            _teamAway._strikerFieldPoints = _awayAttackPoints;
            _teamAway.goalKeeperPoint = GoalAwayPosition;
            _teamAway.matchManager = this;

            _teamHome.goalPostPosition = _goalPostHome.Position;
            _teamHome.oppPostPosition = _goalPostAway.Position;
            _teamHome._midFieldPoints = _homeMidPoints;
            _teamHome._defenderFieldPoints = _homeDefendPoints;
            _teamHome._strikerFieldPoints = _homeAttackPoints;
            _teamHome.goalKeeperPoint = GoalHomePosition;
            _teamHome.matchManager = this;
            _teamHome.IsUserControlledTeam = true;

          
            Player[] awayPlayers = _teamAway.transform.GetComponentsInChildren<Player>();
          
            foreach(Player player in awayPlayers)
            {
                player.OppGoal = _goalPostHome;
                player.TeamGoal = _goalPostAway;
                player.MatchManager = this;
                player.teamChoice = TeamChoice.TeamAway;
                player.joyStick = joyStick;
                player.team = _teamAway;
                player.SetPlayerKit(Statics.playerMaterials[0]);
                //TESTING GOAL KEEPER
                if (player.transform.name.Trim() == "GoalKeeperHighPoly")
                {
                    player.PlayerType = PlayerTypes.Goalkeeper;//TO BE EDITED/SET BEFORE GAME BEGINS
                }

                else if(player.transform.name.Trim() == "Defender")
                {
                    player.PlayerType = PlayerTypes.Defender;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                else
                {
                    
                    player.PlayerType = PlayerTypes.Striker;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                _teamAway.SetupNewPlayer(player);
            }



            Player[] homePlayers = _teamHome.transform.GetComponentsInChildren<Player>();
         
            foreach (Player player in homePlayers)
            {
                player.OppGoal = _goalPostAway;
                player.TeamGoal = _goalPostHome;
                player.MatchManager = this;
                player.teamChoice = TeamChoice.TeamAway;
                player.joyStick = joyStick;
                player.team = _teamHome;
                player.SetPlayerKit(Statics.playerMaterials[1]);
                //TESTING GOAL KEEPER
                if (player.transform.name.Trim() == "GoalKeeperHighPoly")
                {
                    player.PlayerType = PlayerTypes.Goalkeeper;//TO BE EDITED/SET BEFORE GAME BEGINS
                }

                else if (player.transform.name.Trim() == "Defender")
                {
                    player.PlayerType = PlayerTypes.Defender;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                else
                {
                    player.PlayerType = PlayerTypes.Striker;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                _teamHome.SetupNewPlayer(player);
            }
            //startKickOff();
        }

        private void SetPlayersAttributes()
        {
            _teamAway.goalPostPosition = _goalPostAway.Position;
            _teamAway.oppPostPosition = _goalPostHome.Position;
            _teamAway._midFieldPoints = _awayMidPoints;
            _teamAway._defenderFieldPoints = _awayDefendPoints;
            _teamAway._strikerFieldPoints = _awayAttackPoints;
            _teamAway.goalKeeperPoint = GoalAwayPosition;
            _teamAway.matchManager = this;

            _teamHome.goalPostPosition = _goalPostHome.Position;
            _teamHome.oppPostPosition = _goalPostAway.Position;
            _teamHome._midFieldPoints = _homeMidPoints;
            _teamHome._defenderFieldPoints = _homeDefendPoints;
            _teamHome._strikerFieldPoints = _homeAttackPoints;
            _teamHome.goalKeeperPoint = GoalHomePosition;
            _teamHome.matchManager = this;
            _teamHome.IsUserControlledTeam = true;

            Player[] awayPlayers = new Player[11];// _teamAway.transform.GetComponentsInChildren<Player>();
            for(int i=0; i<11; i++)
            {
                if (i==0)
                    awayPlayers[i] = Instantiate(goalKeeperPlayer).GetComponent<Player>();
                else
                    awayPlayers[i]=Instantiate(mainPlayer).GetComponent<Player>();
            }
            int dex = 0;
            foreach(Player player in awayPlayers)
            {
                player.OppGoal = _goalPostHome;
                player.TeamGoal = _goalPostAway;
                player.MatchManager = this;
                player.teamChoice = TeamChoice.TeamAway;
                player.joyStick = joyStick;
                player.team = _teamAway;
                player.Tackling = 1;
                player.SetPlayerKit(Statics.playerMaterials[0]);
                //TESTING GOAL KEEPER
                if (dex==0)//(player.transform.name.Trim() == "GoalKeeperHighPoly")
                {
                    player.PlayerType = PlayerTypes.Goalkeeper;//TO BE EDITED/SET BEFORE GAME BEGINS
                }

                else if (dex>=1 && dex<=5)//(player.transform.name.Trim() == "Defender")
                {
                    player.PlayerType = PlayerTypes.Defender;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                else if(dex>5 && dex < 9)
                {
                    player.PlayerType = PlayerTypes.InFieldPlayer;
                }
                else
                {
                    
                    player.PlayerType = PlayerTypes.Striker;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                _teamAway.SetupNewPlayer(player);
                dex++;
            }

            Player[] homePlayers = new Player[11];//_teamHome.transform.GetComponentsInChildren<Player>();
            for (int i = 0; i < 11; i++)
            {
                if (i == 0)
                    homePlayers[i] = Instantiate(goalKeeperPlayer).GetComponent<Player>();
                else
                    homePlayers[i] = Instantiate(mainPlayer).GetComponent<Player>();
            }
             dex = 0;
            foreach (Player player in homePlayers)
            {
                player.joyStick = joyStick;
                player.MatchManager = this;
                player.teamChoice = TeamChoice.TeamHome;
                player.OppGoal = _goalPostAway;
                player.TeamGoal = _goalPostHome;
                player.joyStick = joyStick;
                player.team = _teamHome;
                player.SetPlayerKit(Statics.playerMaterials[1]);
                //TESTING GOAL KEEPER
                if (dex == 0)//(player.transform.name.Trim() == "GoalKeeperHighPoly")
                {
                    player.PlayerType = PlayerTypes.Goalkeeper;//TO BE EDITED/SET BEFORE GAME BEGINS
                }

                else if (dex >= 1 && dex <= 5)//(player.transform.name.Trim() == "Defender")
                {
                    player.PlayerType = PlayerTypes.Defender;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                else if (dex > 5 && dex < 9)
                {
                    player.PlayerType = PlayerTypes.InFieldPlayer;
                }
                else
                {
                    player.PlayerType = PlayerTypes.Striker;//TO BE EDITED/SET BEFORE GAME BEGINS
                }
                _teamHome.SetupNewPlayer(player);
                dex++;
            }
            UserControlledPlayer = null;
            Ball.Instance.CurrentOwner = null;
           // startKickOff();
        }

        public void startKickOff()
        {
            Player kickOffPlayer1;
            Player kickOffPlayer2;
            int ran1 = Convert.ToInt32((Random.Range(0.1f, 0.2f)) * 10);
            if (ran1 == 1)
            {
                _teamHome.kickOffPlayers[0].PutPlayerAtKickOff(kickOff_1);
                _teamHome.kickOffPlayers[1].PutPlayerAtKickOff(kickOff_2);
                Ball.Instance.CurrentOwner = _teamHome.kickOffPlayers[0];
                Ball.Instance.Position = kickOff_1.position;
              //  _teamHome.kickOffPlayers[0].transform.LookAt(Ball.Instance.Position);
               // _teamHome.kickOffPlayers[1].transform.LookAt(Ball.Instance.Position);
            }
            else
            {
                _teamAway.kickOffPlayers[0].PutPlayerAtKickOff(kickOff_1);
                _teamAway.kickOffPlayers[1].PutPlayerAtKickOff(kickOff_2);
                Ball.Instance.CurrentOwner = _teamAway.kickOffPlayers[0];
                Ball.Instance.Position = kickOff_1.position;
               // _teamAway.kickOffPlayers[0].transform.LookAt(Ball.Instance.Position);
               // _teamAway.kickOffPlayers[1].transform.LookAt(Ball.Instance.Position);
            }
           StartCoroutine(KickOffWait());
            /* if (ran1 == 1)
            {
                kickOffPlayer1 = _teamHome.kickOffPlayers[0];
                kickOffPlayer2 = _teamHome.kickOffPlayers[1];
            }
            else
            {
                kickOffPlayer1 = _teamAway.kickOffPlayers[0];
                kickOffPlayer2 = _teamAway.kickOffPlayers[1];
            }
            kickOffPlayer1 = _teamHome.kickOffPlayers[0];
            kickOffPlayer2 = _teamHome.kickOffPlayers[1];
            kickOffPlayer1.Position = kickOff_1.position;
            kickOffPlayer2.osition = kickOff_2.position;
            Ball.Instance.Position = kickOffPlayer1.transform.position;

            kickOffPlayer1.TackleOwner();
            kickOffPlayer1.transform.LookAt(Ball.Instance.Position);
            kickOffPlayer2.transform.LookAt(Ball.Instance.Position
            */

        }

        private IEnumerator KickOffWait()
        {
           // const float formerTimeScale = Time.timeScale;
            //Time.timeScale = 0;
            yield return new WaitForSeconds(Statics.TimeB4KickOff);
            //Time.timeScale = formerTimeScale;
            Ball.Instance.CurrentOwner.makeRandCross();
        }

        /*
       public void startKickOff(Team team)
       {
           Player kickOffPlayer1;
           Player kickOffPlayer2;
           kickOffPlayer1 = team.kickOffPlayers[0];
           kickOffPlayer2 = team.kickOffPlayers[1];

           kickOffPlayer1.transform.position = kickOff_1.position;
           kickOffPlayer2.transform.position = kickOff_2.position;
           Ball.Instance.Position = kickOffPlayer1.transform.position;

           kickOffPlayer1.TackleOwner();
           kickOffPlayer1.transform.LookAt(Ball.Instance.Position);
           kickOffPlayer2.transform.LookAt(Ball.Instance.Position);

       }*/

        public void postGoal(Goal goalPost)
        {
           
            if (GameHalted) return;
            GameHalted = true;
            TeamChoice scoredChoice=TeamChoice.TeamHome;
            if (goalPost == _goalPostAway)
            {
                _homeScore++;
                if (_homeScoreText != null) _homeScoreText.text = _homeScore.ToString();
                scoredChoice = TeamChoice.TeamAway;
            }
            else if (goalPost == _goalPostHome)
            {
                _awayScore++;
                if (_awayScoreText != null) _awayScoreText.text = _awayScore.ToString();
                scoredChoice = TeamChoice.TeamHome;
            }
           /* Team team = scoredChoice == TeamChoice.TeamHome ? _teamHome : _teamAway;
            if (team.TeamPlayersDefaultPositionMap.ContainsKey(team.goalKeeperPoint))
                team.TeamPlayersDefaultPositionMap[team.goalKeeperPoint].SaveBall();*/
            StartCoroutine(AnimateGoal(scoredChoice));
        }
       
        public void postOwnGoal(Goal goalPost)
        {
           
            if (GameHalted) return;
            GameHalted = true;
            TeamChoice scoredChoice=TeamChoice.TeamHome;
            if (goalPost == _goalPostAway)
            {
                _homeScore++;
                if (_homeScoreText != null) _homeScoreText.text = _homeScore.ToString();
                scoredChoice = TeamChoice.TeamAway;
            }
            else if (goalPost == _goalPostHome)
            {
                _awayScore++;
                if (_awayScoreText != null) _awayScoreText.text = _awayScore.ToString();
                scoredChoice = TeamChoice.TeamHome;
            }
            /* Team team = scoredChoice == TeamChoice.TeamHome ? _teamHome : _teamAway;
  if (team.TeamPlayersDefaultPositionMap.ContainsKey(team.goalKeeperPoint))
      team.TeamPlayersDefaultPositionMap[team.goalKeeperPoint].SaveBall();*/
            StartCoroutine(AnimateOwnGoal(scoredChoice));
        }

        private IEnumerator AnimateOwnGoal(TeamChoice scoredChoice)
        {
            AlertText.text = "OWN GOAL";
            AlertText.transform.gameObject.active = true;
            BroadcastMoveToDefaults();
            yield return new WaitForSeconds(5);
            AlertText.transform.gameObject.active = false;
            RestartWithKickOff(scoredChoice);
            GameHalted = false;
        }

        public void shootByUserControlledPlayer()
        { 
            try
            {
              //  SetDebugText("Ball Just shot"+UserControlledPlayer==null?" no player":" with player" +_shotSlider.value.ToString());

                float shotValue = _shotSlider.value;
                if (UserControlledPlayer != null)
                {
                    //SetDebugText("Player SHOT");
                    UserControlledPlayer.makeShotInForwardDirection(shotValue);
                }
                else
                {
                    //SetDebugText("Player is null");
                }

                _shotSlider.value = _shotSlider.minValue;
            }
            catch (Exception e)
            {
                SetDebugText("Error: "+e.ToString());
            }
        }
        
        public void PassByUserControlledPlayer()
        {          
          if (UserControlledPlayer != null) UserControlledPlayer.makeRandCross();      
        }

        private IEnumerator AnimateGoal(TeamChoice teamChoice)
        {
            
            BroadcastMoveToDefaults();
            _goalText.transform.gameObject.active = true;
            yield return new WaitForSeconds(5);
            _goalText.transform.gameObject.active = false;
            RestartWithKickOff(teamChoice);
            GameHalted = false;
        }

      
        private void RestartWithKickOff(TeamChoice teamChoice)
        {
            //FOR NOW, MAKE GOALKICK
            Team team = teamChoice == TeamChoice.TeamHome ? _teamHome : _teamAway;
            if (team.TeamPlayersDefaultPositionMap.ContainsKey(team.goalKeeperPoint))
                team.TeamPlayersDefaultPositionMap[team.goalKeeperPoint].SaveBall();
            // ENSURE METHOD IS CALLED AFTER GOAL BroadcastReturnFromDefaults();
        }

        internal void BroadcastMoveToDefaults()
        {
            returnToDefaults = true;
            _teamHome.MoveAllPlayersToDefaultPoints();
            _teamAway.MoveAllPlayersToDefaultPoints();
        }

        internal void BroadcastReturnFromDefaults()
        {
            returnToDefaults = false;
            _teamHome.StopMovingToDefaultPoints();
            _teamAway.StopMovingToDefaultPoints();
        }

        public void TriggerLeftThrowIn()
        {
            AlertText.text = "LEFT THROW IN";
            StopMainGameEventsForSecs();
        }

        private void StopMainGameEventsForSecs()
        {
            StartCoroutine(pauseMainGameForSecs(3));
        }

        public void TriggerRightThrowIn()
        {
            AlertText.text = "RIGHT THROW IN";
            StopMainGameEventsForSecs();
        }

        public void FreezeUsertControlledPlayer()
        {
            if (UserControlledPlayer != null) UserControlledPlayer.isFrozen = true;
        }

        public void UnfreezeUserControlledPlayer()
        {
            if (UserControlledPlayer != null) UserControlledPlayer.isFrozen = false;

        }

        public void MakeUserTackle()
        {
            if (UserControlledPlayer != null) UserControlledPlayer.TackleOwner();
        }

        public void SetDebugText(String textVal)
        {
            if(DebugText!=null)
            DebugText.text = textVal;
        }

        private IEnumerator pauseMainGameForSecs(float secs)
        {
            AlertText.gameObject.active = true;
            yield return new WaitForSeconds(secs);
            AlertText.gameObject.active = false;
        }

        internal void TriggerHomeGoalKick()
        {
            AlertText.text = "HOME GOAL KICK";
            StartCoroutine(WaitAndSaveGoalKick(_teamHome));
            //StopMainGameEventsForSecs();
        }
        
        internal void TriggerAwayGoalKick()
        {
            AlertText.text = "AWAY GOAL KICK";
            StartCoroutine(WaitAndSaveGoalKick(_teamAway));
        }

        private IEnumerator WaitAndSaveGoalKick(Team team)
        {
            Transform goalPosition = team == _teamHome ? GoalHomePosition : GoalAwayPosition;
            if (team.TeamPlayersDefaultPositionMap.ContainsKey(goalPosition))
                team.TeamPlayersDefaultPositionMap[goalPosition].SaveBall();
            AlertText.gameObject.active = true;
            yield return new WaitForSeconds(Statics.TimeWaitAfterGoalSaved);
            AlertText.gameObject.active = false;
     
        }

        internal void TriggerCornerKick()
        {
            print("routine started");
            AlertText.text = "Corner Kick";
            StartCoroutine(pauseMainGameForSecs(6));
        }

        public Team TeamAway { get => _teamAway; }
        public Team TeamHome { get => _teamHome; }

        /// <summary>
        /// Property to access the team root transform
        /// </summary>
        public Transform RootTeam { get => _rootTeam; }
        public Transform TransformCentreSpot { get => _transformCentreSpot; set => _transformCentreSpot = value; }
        public InGameTeamDto AwayTeamData { get => _awayTeamData; set => _awayTeamData = value; }
        public InGameTeamDto HomeTeamData { get => _homeTeamData; set => _homeTeamData = value; }
        public ThrowInTrigger LeftThrowInTrigger { get => _leftThrowInTrigger; set => _leftThrowInTrigger = value; }
        public ThrowInTrigger RightThrowInTrigger { get => _rightThrowInTrigger; set => _rightThrowInTrigger = value; }
        public MatchDifficultyTeamParam CpuTeamParams { get => _cpuTeamParams; set => _cpuTeamParams = value; }
        public MatchDifficultyTeamParam UserTeamParams { get => _userTeamParams; set => _userTeamParams = value; }
    }
}
