using Assets.FootballGameEngine_Indie.Scripts.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.StateMachines.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.States.Managers.GameManagerStates.HomeState.MainState;
using RobustFSM.Base;
using static Assets.FootballGameEngine_Indie_.Scripts.UI.Menus.PrepareForMatchMenu.SubMenus.SelectTeamsSubMenu;

namespace Assets.FootballGameEngine_Indie_.Scripts.States.Managers.GameManagerStates.PrepareForMatchState.SubStates
{
    public class SelectTeams : BState
    {
        public override void Initialize()
        {
            base.Initialize();

            // initialize stuff
            Init();
        }

        public override void Enter()
        {
            base.Enter();

            InitializeUtilityMenu();

            // get the selected items
            OnSelectTeam(ref Owner.SelectedCpuTeamId, 0, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.CpuControlledTeamInfo);
            OnSelectTeam(ref Owner.SelectedUserTeamId, 0, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.UserControlledTeamInfo);
            
            // enable the select teams submenu
            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.Root.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();

            // disable the select teams submenu
            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.Root.SetActive(false);
        }

        public GameManager Owner
        {
            get
            {
                return ((GameManagerFSM)SuperMachine).Owner;
            }
        }

        private void Init()
        {
            InitCpuTeamInfoUI();
            InitUserTeamInfoUI();
        }

        private void InitCpuTeamInfoUI()
        {
            GraphicsManager.Instance
                .PrepareForMatchMainMenu
                .SelectTeamSubMenu
                .CpuControlledTeamInfo
                .BtnSelectNextTeam
                .onClick
                .AddListener(delegate ()
            {
                OnSelectNextTeam(ref Owner.SelectedCpuTeamId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.CpuControlledTeamInfo);
            });

            GraphicsManager.Instance
                .PrepareForMatchMainMenu
                .SelectTeamSubMenu
                .CpuControlledTeamInfo
                .BtnSelectPrevTeam
                .onClick
                .AddListener(delegate ()
            {
                OnSelectPrevTeam(ref Owner.SelectedCpuTeamId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.CpuControlledTeamInfo);
            });
        }

        private void InitUserTeamInfoUI()
        {
            GraphicsManager.Instance
                .PrepareForMatchMainMenu
                .SelectTeamSubMenu
                .UserControlledTeamInfo
                .BtnSelectNextTeam
                .onClick
                .AddListener(delegate ()
            {
                OnSelectNextTeam(ref Owner.SelectedUserTeamId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.UserControlledTeamInfo);
            });

            GraphicsManager.Instance
                .PrepareForMatchMainMenu
                .SelectTeamSubMenu
                .UserControlledTeamInfo
                .BtnSelectPrevTeam
                .onClick
                .AddListener(delegate ()
            {
                OnSelectPrevTeam(ref Owner.SelectedUserTeamId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectTeamSubMenu.UserControlledTeamInfo);
            });
        }

        private void InitializeUtilityMenu()
        {
            Owner.InitializeUtilityMenu(true, true, "Select Teams", 
            delegate
            {
                // play sound
                Owner.OnButtonClicked();
                SuperMachine.ChangeState<HomeMainState>();
            }, 
            delegate
            {
                // play sound
                Owner.OnButtonClicked();
                Machine.ChangeState<SelectTeamsKits>();
            });
        }

        private void OnSelectNextTeam(ref int teamIndex, TeamInfo teamInfo)
        {
            // play sound
            Owner.OnButtonClicked();
            OnSelectTeam(ref teamIndex, 1, teamInfo);
        }

        private void OnSelectPrevTeam(ref int teamIndex, TeamInfo teamInfo)
        {
            // play sound
            Owner.OnButtonClicked();
            OnSelectTeam(ref teamIndex, -1, teamInfo);
        }

        private void OnSelectTeam(ref int teamIndex, int step, TeamInfo teamInfo)
        {
            // update the team index by step
            teamIndex += step;

            // check if team index is now below 0
            bool isIndexBelowRange = teamIndex < 0;

            // wrap team index if below zero
            if (isIndexBelowRange)
                teamIndex = Owner.TeamsData.Count - 1;
            
            // check if index has overshoot range
            bool isIndexAboveRange = teamIndex > Owner.TeamsData.Count - 1;

            // wrap index if it has overshoot range
            if (isIndexAboveRange)
                teamIndex = 0;
            
            // get the team dto
            TeamDto selectedTeam = Owner.TeamsData[teamIndex];

            // update the ui
            teamInfo.ImgTeamIcon.sprite = selectedTeam.Icon;
            teamInfo.TxtTeamName.text = selectedTeam.Name;

            teamInfo.TxtTeamFormation.text = FormationManager.Instance.GetFormation(selectedTeam.FormationType).FormationName;

            teamInfo.TxtTeamAttack.text = Owner.GetTeamAttackTactic(selectedTeam.AttackType).Name;
            teamInfo.TxtTeamDefence.text = Owner.GetTeamDefenceTactic(selectedTeam.DefenceType).Name;
        }
    }
}
