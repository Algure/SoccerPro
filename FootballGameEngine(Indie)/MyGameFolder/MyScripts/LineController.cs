using Assets.FootballGameEngine_Indie.Scripts.Entities;
using Assets.FootballGameEngine_Indie.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    MatchManager _matchManager;
    Player player;
    LineRenderer _renderer;
    Vector3 _playerPos;
    Vector3 _renderPos;
    // Start is called before the first frame update
    void Start()
    {
        _matchManager = GameObject.Find("Manager").GetComponent<MatchManager>();
        _renderer = this.transform.GetComponent<LineRenderer>();
        //_renderer.material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (_matchManager.UserControlledPlayer != null)
        {
            _playerPos = _matchManager.UserControlledPlayer.transform.position;
            _renderPos = _playerPos + (_matchManager.UserControlledPlayer.transform.forward * 200);
            _renderer.positionCount = 2;
            _renderer.SetPositions(new Vector3[] { _playerPos, _renderPos});
        }
    }
}
