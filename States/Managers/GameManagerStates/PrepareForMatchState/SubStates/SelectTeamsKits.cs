using Assets.FootballGameEngine_Indie.Scripts.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.StateMachines.Managers;
using Assets.FootballGameEngine_Indie_.Scripts.States.Managers.GameManagerStates.GameOnState.MainState;
using Assets.FootballGameEngine_Indie_.Scripts.UI.Menus.PrepareForMatchMenu.SubMenus;
using RobustFSM.Base;

namespace Assets.FootballGameEngine_Indie_.Scripts.States.Managers.GameManagerStates.PrepareForMatchState.SubStates
{
    public class SelectTeamsKits : BState
    {
        public override void Initialize()
        {
            base.Initialize();

            // init 
            Init();
        }

        public override void Enter()
        {
            base.Enter();

            InitializeUtilityMenu();

            OnSelectTeamKit(Owner.SelectedCpuTeamId, ref Owner.SelectedCpuTeamKitId, 0, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.CpuControlledTeamInfo);
            OnSelectTeamKit(Owner.SelectedUserTeamId, ref Owner.SelectedUserTeamKitId, 0, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.UserControlledTeamInfo);

            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.Root.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();

            // disable the select kits root
            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.Root.SetActive(false);
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
            InitCpuTeamKitUI();
            InitUserTeamKitUI();
        }

        private void InitCpuTeamKitUI()
        {
            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.CpuControlledTeamInfo.BtnSelectNextTeam.onClick.AddListener(delegate ()
            {
                OnSelectNextTeamKit(Owner.SelectedCpuTeamId, ref Owner.SelectedCpuTeamKitId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.CpuControlledTeamInfo);
            });

            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.CpuControlledTeamInfo.BtnSelectPrevTeam.onClick.AddListener(delegate ()
            {
                OnSelectPrevTeamKit(Owner.SelectedCpuTeamId, ref Owner.SelectedCpuTeamKitId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.CpuControlledTeamInfo);
            });
        }

        private void InitUserTeamKitUI()
        {
            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.UserControlledTeamInfo.BtnSelectNextTeam.onClick.AddListener(delegate ()
            {
                OnSelectNextTeamKit(Owner.SelectedUserTeamId, ref Owner.SelectedUserTeamKitId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.UserControlledTeamInfo);
            });

            GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.UserControlledTeamInfo.BtnSelectPrevTeam.onClick.AddListener(delegate ()
            {
                OnSelectPrevTeamKit(Owner.SelectedCpuTeamId, ref Owner.SelectedCpuTeamKitId, GraphicsManager.Instance.PrepareForMatchMainMenu.SelectKitsSubMenu.UserControlledTeamInfo);
            });
        }

        private void OnSelectNextTeamKit(int teamIndex, ref int teamKitIndex, SelectTeamsSubMenu.TeamInfo teamInfo)
        {
            // play sound
            Owner.OnButtonClicked();
            OnSelectTeamKit(teamIndex, ref teamKitIndex, 1, teamInfo);
        }

        private void OnSelectPrevTeamKit(int teamIndex, ref int teamKitIndex, SelectTeamsSubMenu.TeamInfo teamInfo)
        {
            // play sound
            Owner.OnButtonClicked();
            OnSelectTeamKit(teamIndex, ref teamKitIndex, -1, teamInfo);
        }

        private void OnSelectTeamKit(int teamIndex, ref int teamKitIndex, int step, SelectTeamsSubMenu.TeamInfo teamInfo)
        {
            // update the index
            teamKitIndex += step;

            // check if team index is below range
            bool isIndexBelowRange = teamKitIndex < 0;

            // wrap index if below range
            if (isIndexBelowRange)
                teamKitIndex = Owner.TeamsData[teamIndex].Kits.Count - 1;
            
            // check if index is above rang
            bool isIndexAboveRange = teamKitIndex > Owner.TeamsData[teamIndex].Kits.Count - 1;

            // wrap if index is above range
            if (isIndexAboveRange)
                teamKitIndex = 0;
            
            // update ui
            KitDto selectedTeam = Owner.TeamsData[teamIndex].Kits[teamKitIndex];
            teamInfo.ImgTeamIcon.sprite = selectedTeam.ImgIcon;
            teamInfo.TxtTeamName.text = selectedTeam.Name;
        }

        private void InitializeUtilityMenu()
        {
            Owner.InitializeUtilityMenu(true, 
                true, 
                "Select Kits", 
                delegate
                {
                    // play sound
                    Owner.OnButtonClicked();
                    Machine.ChangeState<SelectTeams>();
                },
                delegate
                {
                    // play sound
                    Owner.OnButtonClicked();
                    Machine.ChangeState<PreMatch>();
                });
        }
    }
}
