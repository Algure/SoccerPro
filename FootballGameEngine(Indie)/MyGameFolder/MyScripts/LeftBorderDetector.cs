using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftBorderDetector : MonoBehaviour
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
        if (other.transform.name.Trim() == "Ball")
        {
            _matchManager.TriggerLeftThrowIn();
            return;
        }

        Player otherPlayer = other.transform.GetComponentInParent<Player>();
        if (otherPlayer == null) return;

        if (otherPlayer == Ball.Instance.CurrentOwner) _matchManager.TriggerLeftThrowIn();

    }
}
