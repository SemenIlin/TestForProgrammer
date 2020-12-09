using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using EpPathFinding.cs;

public class GameLogic : MonoBehaviour
{
    private const float MAX_DISTANCE = 0.01f;
    private const float MIN_DISTANCE_BETWEEN_PLAYERS = 1f;

    [SerializeField] PeopleFactory _peopleFactory;
    [SerializeField] ZombiFactory _zombiFactory;
    [SerializeField] GameField _gameField;

    private List<Player> _players;

    private Dictionary<Player, IEnumerator<Vector3>> _path;
    
    private List<Vector3> pathVector3;

    private void Start()
    {
        pathVector3 = new List<Vector3>();
        _path = new Dictionary<Player, IEnumerator<Vector3>>();

        _players = new List<Player>();

        _players.Add(_zombiFactory.Get(SpecializationType.archer));
        _players.Add(_zombiFactory.Get(SpecializationType.swordsman));

        _players.Add(_peopleFactory.Get(SpecializationType.swordsman));
        _players.Add(_peopleFactory.Get(SpecializationType.archer));
    }

    private void Update()
    {

        for (var i = 0; i < _players.Count; ++i)
        {

            if (_players[i].PathVector == null || _players[i].PathVector.Count == 0)
            {
                continue;
            }

            if (_path[_players[i]].Current == null)
            {
                continue;
            }

            _players[i].Transform.position = Vector3.MoveTowards(_players[i].Transform.position,
                                                                 _path[_players[i]].Current,
                                                                 _players[i].MoveSpeed * Time.deltaTime);

            var distanceI = (_players[i].Transform.position - _path[_players[i]].Current).sqrMagnitude;

            if (!IsComeAcross(_players[i], _players[_players[i].IndexMoveToEnemy]))
            {
                if (distanceI < MAX_DISTANCE)
                {
                    _path[_players[i]].MoveNext();
                    MoveToGoal();
                }
            } 
        }
    }

    public void MoveToGoal()
    {
        CreatePathes();
        foreach (var player in _players)
        {
            _path[player].MoveNext();
            _path[player].MoveNext();
        }
    }

    public void CreatePathes()
    {
        _path.Clear();
        SearchForNearbyEnemy();
        
        foreach (var player in _players)
        {
            var y = player.Transform.position.y;

            var startPosition = new GridPos((int)Math.Round(player.Transform.position.x),
                                            (int)Math.Round(player.Transform.position.z));

            var endPosition = new GridPos((int)Math.Round(_players[player.IndexMoveToEnemy].Transform.position.x),
                                          (int)Math.Round(_players[player.IndexMoveToEnemy].Transform.position.z));

            var jpParam = new JumpPointParam(_gameField.BaseGrid, startPosition, endPosition);
            var path = JumpPointFinder.FindPath(jpParam);

            SetOccupiedCells(path);
            ConvertToVector3Position(path, y);

            player.PathVector.Copy(pathVector3);

            _path.Add(player, GetNextPosition(player.PathVector));
            pathVector3.Clear();
        }
    } 

    public void SearchForNearbyEnemy()
    {
        float distance;
        float tempararyDistance;
        for (var i = 0; i < _players.Count; ++i)
        {
            distance = float.MaxValue;
            for (var j = 0; j < _players.Count; ++j)
            {
                if (i == j)
                    continue;

                if (_players[i].RaceType == _players[j].RaceType)
                    continue;

                tempararyDistance = (_players[i].Transform.position - _players[j].Transform.position).magnitude;
                if (distance > tempararyDistance)
                {
                    distance = tempararyDistance;
                    _players[i].IndexMoveToEnemy = j;
                }
            }
        }
    }
    
    private void ConvertToVector3Position(List<GridPos> gridPos, float y)
    {
        for (var i = 0; i < gridPos.Count; ++i)
        {
            pathVector3.Add(new Vector3(gridPos[i].x, y, gridPos[i].y));
        }
    }
    private void SetOccupiedCells(List<GridPos> gridPos)
    {
        foreach(var pos in gridPos)
        {
            _gameField.BaseGrid.GetNodeAt(pos).Reset();
        }
    }

    private bool IsComeAcross(Player a, Player b)
    {
        return (a.Transform.position - b.Transform.position).magnitude < MIN_DISTANCE_BETWEEN_PLAYERS ? true : false;
    }

    private IEnumerator<Vector3> GetNextPosition(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0)
            yield break;

        var movingTo = 0;
        var direction = 1;

        while (true)
        {
            yield return positions[movingTo];
            if(positions.Count - 1 > movingTo)
            {
                movingTo += direction;
            }
        }       
    }
}
