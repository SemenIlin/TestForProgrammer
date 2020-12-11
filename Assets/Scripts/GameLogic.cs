using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using EpPathFinding.cs;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    [SerializeField] PeopleFactory _peopleFactory;
    [SerializeField] ZombiFactory _zombiFactory;
    [SerializeField] GameField _gameField;
    [SerializeField] Text _count;

    private List<Vector3> pathVector3;
    public List<Player> Players { get; private set; }
    public Dictionary<Player, IEnumerator<Vector3>> Path { get; private set; }   

    private void Start()
    {
        pathVector3 = new List<Vector3>();
        Path = new Dictionary<Player, IEnumerator<Vector3>>();

        Players = new List<Player>();

        var min = 20;
        var max = 30;

        StartCoroutine(SpawnPlayers(min, max, _peopleFactory));
        StartCoroutine(SpawnPlayers(min, max, _zombiFactory));
    }

    /// <summary>
    /// Create path for concrete player. Use after Change position.
    /// </summary>
    /// <param name="player"></param>
    public void CreatePathForNearbyEnemy(Player player)
    {
        SearchPathForNearbyEnemy(player);

        if (Path.ContainsKey(player))
            ChangePath(player);
        else
            AddPath(player);
    }

    /// <summary>
    /// Create pathes for all player of concrete race. Use after Death and Spawn player.
    /// </summary>
    /// <param name="player"></param>
    public void CreatePathesForNearbyEnemy(Player player)
    {
        float distance;
        float tempararyDistance;

        for (var i = 0; i < Players.Count; ++i)
        {
            Players[i].IndexMoveToEnemy = null;
            distance = float.MaxValue;

            if (Players[i].RaceType != player.RaceType)
                continue;

            for (var j = 0; j < Players.Count; ++j)
            {
                if (i == j)
                    continue;

                if (Players[i].RaceType == Players[j].RaceType)
                    continue;

                tempararyDistance = (Players[i].Transform.position - Players[j].Transform.position).magnitude;
                if (distance > tempararyDistance)
                {
                    distance = tempararyDistance;
                    Players[i].IndexMoveToEnemy = j;
                }
            }

            if (Path.ContainsKey(Players[i]))
                ChangePath(Players[i]); 
            else
                AddPath(Players[i]);
        }
    }
    public void SearchPathForNearbyEnemy(Player player)
    {
        float distance;
        float tempararyDistance;

        player.IndexMoveToEnemy = null;
        distance = float.MaxValue;
        
        for (var j = 0; j < Players.Count; ++j)
        {
            if (player.RaceType == Players[j].RaceType)
                continue;

            tempararyDistance = (player.Transform.position - Players[j].Transform.position).magnitude;
            if (distance > tempararyDistance)
            {
                distance = tempararyDistance;
                player.IndexMoveToEnemy = j;
            }
        }
    }

    public void TakeDamage(Player causeDamage, Player takesDamage)
    {
        takesDamage.Health -= causeDamage.Damage;
        if (takesDamage.Health <= 0)
        {
            Debug.Log("Death");
            DestoyPlayer(takesDamage, causeDamage);
        }
    }

    public void RemovePath(Player player)
    {
        Path.Remove(player);
    }
    private void ChangePath(Player player)
    {
        AddPathVectorForPlayer(player);
        Path[player] = GetNextPosition(player.PathVector);

        Path[player].MoveNext();
        Path[player].MoveNext();
    }
    private void AddPath(Player player) 
    {
        AddPathVectorForPlayer(player);
        if (player.PathVector == null)
            return;
        Path.Add(player, GetNextPosition(player.PathVector));

        Path[player].MoveNext();
        Path[player].MoveNext();
    }


    private void AddPathVectorForPlayer(Player player)
    {
        var path = CreatePath(player);
        if (path == null)
            return;

        var y = player.Transform.position.y;
        SetOccupiedCells(path);
        ConvertToVector3Position(path, y);

        player.PathVector.Clear();
        player.PathVector.Copy(pathVector3);
        pathVector3.Clear();
    }

    private List<GridPos> CreatePath(Player player)
    {
        if (!player.IndexMoveToEnemy.HasValue)
            return null;

        var startPosition = new GridPos((int)Math.Round(player.Transform.position.x),
                                        (int)Math.Round(player.Transform.position.z));
        var endPosition = new GridPos((int)Math.Round(Players[player.IndexMoveToEnemy.Value].Transform.position.x),
                                      (int)Math.Round(Players[player.IndexMoveToEnemy.Value].Transform.position.z));

        if (player.JumpPointParam == null)
            player.JumpPointParam = new JumpPointParam(_gameField.BaseGrid, startPosition, endPosition);
        else
            player.JumpPointParam.Reset(startPosition, endPosition);

        return JumpPointFinder.FindPath(player.JumpPointParam);
    } 

    private void DestoyPlayer(Player takesDamage, Player causeDamage)
    {
        RemovePath(takesDamage);
        Players.Remove(takesDamage);
        _count.text = Players.Count.ToString();
        Destroy(takesDamage.gameObject);

        CreatePathesForNearbyEnemy(causeDamage);
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

    public bool IsComeAcross(Player a, Player b)
    {
        return (a.Transform.position - b.Transform.position).magnitude < a.Distance ? true : false;
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
    private IEnumerator SpawnPlayers(int min, int max, IFactory<Player> factory)
    {
        while (true)
        {
            var playerArcher = factory.Get(SpecializationType.archer);
            var positionArcher = new Vector3(UnityEngine.Random.Range(min / 2, max),
                                       playerArcher.Transform.position.y,
                                       UnityEngine.Random.Range(min / 2, max));
            playerArcher.Transform.position = positionArcher;
            Players.Add(playerArcher);
            CreatePathForNearbyEnemy(playerArcher);

            var playerSwordman = factory.Get(SpecializationType.swordsman);
            var position = new Vector3(UnityEngine.Random.Range(min / 2, max),
                                       playerSwordman.Transform.position.y,
                                       UnityEngine.Random.Range(min / 2, max));
            playerSwordman.Transform.position = position;
            Players.Add(playerSwordman);
            _count.text = Players.Count.ToString();
            CreatePathForNearbyEnemy(playerSwordman);
            CreatePathesForNearbyEnemy(playerSwordman);

            yield return new WaitForSeconds(3f);
        }
    }
}
