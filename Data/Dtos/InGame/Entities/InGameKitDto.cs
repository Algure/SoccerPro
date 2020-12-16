using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.FootballGameEngine_Indie_.Scripts.Data.Dtos.InGame.Entities
{
    [Serializable]
    public class InGameKitDto
    {
        [SerializeField]
        Texture _goalKeeperKit;

        [SerializeField]
        Texture _inFieldPlayerKit;

        public Texture GoalKeeperKit { get => _goalKeeperKit; set => _goalKeeperKit = value; }
        public Texture InFieldPlayerKit { get => _inFieldPlayerKit; set => _inFieldPlayerKit = value; }

        public InGameKitDto(Dtos.Entities.KitDto kitDto)
        {
            _goalKeeperKit = kitDto.GoalKeeperKit;
            _inFieldPlayerKit = kitDto.InFieldPlayerKit;
        }

    }
}
