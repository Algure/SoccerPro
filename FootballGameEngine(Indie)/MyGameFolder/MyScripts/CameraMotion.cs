using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [SerializeField]
    Transform _ball;

    [SerializeField]
    MatchManager matchManager;

    Ball ball;

    float nextCamYPosition;
    Vector3 nextCamPosition;
    Vector3 offSet;
    Vector3 offSetStart;
    bool _doneMoving;
    // Start is called before the first frame update
    void Start()
    {
        ball = _ball.GetComponent<Ball>();
        _doneMoving = false;
        offSetStart = transform.position - _ball.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_doneMoving)
        {
            offSet = transform.position - _ball.position;
            if (offSet.magnitude >= 16)
                transform.position = Vector3.MoveTowards(transform.position, _ball.position, 0.08f);
            else
            {
                _doneMoving = true;
                nextCamYPosition = transform.position.y;
            }
        }
        else
        {
            if (matchManager.returnToDefaults && !matchManager.isToKickOff) return;

            if (ball.CurrentOwner == null)
            {
                nextCamPosition = offSet + _ball.position;
            }
            else {

                nextCamPosition = offSet + Ball.Instance.CurrentOwner.transform.position;
            }
            nextCamPosition.y = nextCamYPosition;
            transform.position = nextCamPosition;
        }   
    }
}
