using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie_.Scripts.Triggers;

namespace Assets.FootballGameEngine_Indie.Scripts.Triggers
{
    public class GoalTrigger : BTrigger
    {
        public Goal Goal;// { get; set; }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            print("collision detected");
            if(other.gameObject.name.Trim()== "Ball") Goal.triggerGoal();
        }
    }
}
