using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeGoalBorderDetector : MonoBehaviour
{
    [SerializeField]
    MatchManager _matchManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(UnityEngine.Collider other)
    {
        print("collision detected: " + other.name);
        if (other.transform.name.Trim() == "Ball")
        {
            Player formerOwner = Ball.Instance.OwnerWithLastTouch;
            if (formerOwner!=null && formerOwner.teamChoice == TeamChoice.TeamAway)
            {
                _matchManager.TriggerHomeGoalKick();
            }
            else
            {
                //_matchManager.TriggerHomeGoalKick();
               _matchManager.TriggerCornerKick();
            }
            return;
        }
     
        Player otherPlayer = other.transform.GetComponentInParent<Player>();
        if (otherPlayer == null) return;

        if (otherPlayer == Ball.Instance.CurrentOwner)
        {
            Player formerOwner = otherPlayer;// Ball.Instance.OwnerWithLastTouch;
            if (formerOwner != null && formerOwner.teamChoice == TeamChoice.TeamAway)
            {
                _matchManager.TriggerHomeGoalKick();
            }
            else
            {
                //_matchManager.TriggerHomeGoalKick();
                _matchManager.TriggerCornerKick();
            }
        }
    }
}
