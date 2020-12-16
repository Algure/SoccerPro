using SmartMenuManagement.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.FootballGameEngine_Indie_.Scripts.UI.Menus.HomeMenu.MainMenu
{
    [Serializable]
    public class HomeMainMenu : BMenu
    {
        [SerializeField]
        private Button _btnExit;

        [SerializeField]
        private Button _btnKickOff;

        [SerializeField]
        private Button _btnSettings;

        public Button BtnExit { get => _btnExit; set => _btnExit = value; }
        public Button BtnKickOff { get => _btnKickOff; set => _btnKickOff = value; }
        public Button BtnSettings { get => _btnSettings; set => _btnSettings = value; }
    }
}
