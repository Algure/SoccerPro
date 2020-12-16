using SmartMenuManagement.Scripts;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.FootballGameEngine_Indie_.Scripts.UI.Menus.GameOnMenu.SubMenus.MatchInPlayMenu.SubMenus
{
    [Serializable]
    public class MatchPlayMenu : BSubMenu
    {
        [SerializeField]
        Button _btnPauseMatch;

        [SerializeField]
        Text _txtScores;

        [SerializeField]
        Text _txtTime;

        public Text TxtScores { get => _txtScores; set => _txtScores = value; }
        public Text TxtTime { get => _txtTime; set => _txtTime = value; }
        public Button BtnPauseMatch { get => _btnPauseMatch; set => _btnPauseMatch = value; }

        public void Init(int scoresTeamAway, int scoresTeamHome, string nameAwayTeam, string nameHomeTeam, string time)
        {
            _txtScores.text = string.Format("{0} {1}-{2} {3}", nameAwayTeam, scoresTeamAway, scoresTeamHome, nameHomeTeam);
            _txtTime.text = time;
        }
    }
}
