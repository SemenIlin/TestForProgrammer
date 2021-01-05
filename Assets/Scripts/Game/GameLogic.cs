using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using EpPathFinding.cs;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    public const float INTERVAL = 5f;
    private const float MAX_DISTANCE = 0.01f;
    private const float PERIOD_FOR_UPDATE = 1.5f;
    private const int SIZE_OF_ARMY = 200;
    
    [SerializeField] private PeopleFactory _peopleFactory;
    [SerializeField] private ZombiFactory _zombiFactory;
    private GameField _gameField;

    private int _totalQuantityPlayers;
    private int _index;
    private List<Vector3> pathVector3;

    private List<Player> _peopleSwordmans;
    private List<Player> _peopleArchers;

    private List<Player> _zombieSwordmans;
    private List<Player> _zombieArchers;
    public List<Player> Players { get; private set; }    
    public Dictionary<Player, IEnumerator<Vector3>> Path { get; private set; }

    public static GameLogic Instance;
    public static event Action<int> UpdateQuantityPlayers;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
    }

    private void Start()
    {
        InitArmy();
        CreatedArmy(_zombiFactory, SpecializationType.archer, _zombieArchers, SIZE_OF_ARMY);
        CreatedArmy(_zombiFactory, SpecializationType.swordsman, _zombieSwordmans, SIZE_OF_ARMY);
        CreatedArmy(_peopleFactory, SpecializationType.archer, _peopleArchers, SIZE_OF_ARMY);
        CreatedArmy(_peopleFactory, SpecializationType.swordsman, _peopleSwordmans, SIZE_OF_ARMY);

        _index = 0;
        _gameField = GameField.Instance;
        pathVector3 = new List<Vector3>();
        Path = new Dictionary<Player, IEnumerator<Vector3>>();

        Players = new List<Player>();

        var max = _gameField.SizeBoard.x <= _gameField.SizeBoard.y ? _gameField.SizeBoard.y :
                                                                     _gameField.SizeBoard.x;

        StartCoroutine(SpawnPlayers(0, max - 2, _peopleArchers, _peopleSwordmans));
        StartCoroutine(SpawnPlayers(0, max - 2, _zombieArchers, _zombieSwordmans));
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        for (var i = 0; i < Players.Count; ++i)
        {
            if (Players[i] == null)
                continue;

            if (!Players[i].IsTakeDamage)
            {
                if (!Players[i].IndexMoveToEnemy.HasValue)
                {
                    CreatePathForNearbyEnemy(Players[i]);
                    continue;
                }
                if (Players[Players[i].IndexMoveToEnemy.Value] == null)
                    continue;

                if (!Path.ContainsKey(Players[i]) || 
                    Path[Players[i]] == null)
                    continue;

                var distance = (Players[i].Transform.position - Path[Players[i]].Current).sqrMagnitude;
                
                if (!IsComeAcross(Players[i], Players[Players[i].IndexMoveToEnemy.Value]))
                {
                    Players[i].Transform.position = Vector3.MoveTowards(Players[i].Transform.position,
                                                                    Path[Players[i]].Current,
                                                                    Players[i].MoveSpeed * deltaTime);
                    if (distance < MAX_DISTANCE)
                    {
                        CreatePathForNearbyEnemy(Players[i]);
                        if (Path[Players[i]] == null)
                            continue;

                        Path[Players[i]].MoveNext();
                    }
                    else
                    {
                        Players[i].TimeUntilNextUpdate += deltaTime;
                        if (Players[i].TimeUntilNextUpdate > PERIOD_FOR_UPDATE)
                        {
                            Players[i].TimeUntilNextUpdate = 0;
                            CreatePathForNearbyEnemy(Players[i]);
                        }
                    }
                }
                else
                {
                    Players[i].IsTakeDamage = true;
                    StartCoroutine(Players[i].TakeDamage.Damage());
                }
            }
        }
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
    /// Create pathes for all player without target. Use after Death and Spawn player.
    /// </summary>
    /// <param name="player"></param>
    public void CreatePathesForNearbyEnemy(List<Player> players)
    {
        float distance;
        float tempararyDistance;
        foreach (var player in players)
        {
            if (player == null)
                continue;

            player.IndexMoveToEnemy = null;
            distance = float.MaxValue;
            foreach (var playerJ in Players)
            {
                if (playerJ == null || player.RaceType == playerJ.RaceType)
                    continue;

                tempararyDistance = (player.Transform.position - playerJ.Transform.position).magnitude;
                if (distance > tempararyDistance)
                {
                    distance = tempararyDistance;
                    player.IndexMoveToEnemy = playerJ.IndexInMap;
                }
            }
            if (Path.ContainsKey(player))
                ChangePath(player);
            else
                AddPath(player);
        }
    }
    public void SearchPathForNearbyEnemy(Player player)
    {
        if (player == null)
            return;

        float distance;
        float tempararyDistance;

        player.IndexMoveToEnemy = null;
        distance = float.MaxValue;

        foreach (var item in Players)
        {
            if (item == null || player.RaceType == item.RaceType)
                continue;

            tempararyDistance = (player.Transform.position - item.Transform.position).magnitude;
            if (distance > tempararyDistance)
            {
                distance = tempararyDistance;
                player.IndexMoveToEnemy = item.IndexInMap;
            }
        }
    }

    public void TakeDamage(Player causeDamage, Player takesDamage)
    {
        takesDamage.Health -= causeDamage.Damage;
        if (takesDamage.Health <= 0)
        {
            DestoyPlayer(takesDamage);
        }
    }

    private List<Player> GetPlayrsWithoutTarget(Player takesDamage)
    {
        return Players.Where(player => player != null && player.IndexMoveToEnemy == takesDamage.IndexInMap)
                      .ToList();
    }
    private void ChangePath(Player player)
    {
        AddPathVectorForPlayer(player);
        if (player.PathVector == null)
        {
            Path[player] = null;
            return;
        }
        Path[player] = GetNextPosition(player.PathVector);

        Path[player].MoveNext();
        Path[player].MoveNext();
    }
    private void AddPath(Player player) 
    {
        AddPathVectorForPlayer(player);
        if (player.PathVector == null || player == null)
            return;
        Path.Add(player, GetNextPosition(player.PathVector));

        Path[player].MoveNext();
        Path[player].MoveNext();
    }

    private void AddPathVectorForPlayer(Player player)
    {
        var path = CreatePath(player);
        if (path == null)
        {
            player.PathVector.Clear();
            pathVector3.Clear();
            return;
        }

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

       
        var  jpParam = new JumpPointParam(_gameField.BaseGrid, startPosition, endPosition);

        return JumpPointFinder.FindPath(jpParam);
    } 

    private void DestoyPlayer(Player takesDamage)
    {
        Path.Remove(takesDamage);
        DisatcivatePlayer(takesDamage);
        Players[takesDamage.IndexInMap] = null;

        CreatePathesForNearbyEnemy(GetPlayrsWithoutTarget(takesDamage));
        --_totalQuantityPlayers;
        UpdateQuantityPlayers?.Invoke(_totalQuantityPlayers);
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

    private void DisatcivatePlayer(Player takesDamage)
    {
        Player disactivate = null;
        switch (takesDamage.RaceType)
        {
            case RaceType.people:
                if (takesDamage.SpecializationType == SpecializationType.swordsman)
                {
                    disactivate = _peopleSwordmans[takesDamage.IndexInList];
                }
                else if (takesDamage.SpecializationType == SpecializationType.archer)
                {
                    disactivate = _peopleArchers[takesDamage.IndexInList];
                }
                break;
            case RaceType.zombi:
                if (takesDamage.SpecializationType == SpecializationType.swordsman)
                {
                    disactivate = _zombieSwordmans[takesDamage.IndexInList];
                }
                else if (takesDamage.SpecializationType == SpecializationType.archer)
                {
                    disactivate = _zombieArchers[takesDamage.IndexInList];
                }
                break;
        }
        disactivate.Transform.position = new Vector3(-100, disactivate.Transform.position.y, -100);
        disactivate.enabled = false;
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

    private void AddPlayer(Player player, int indexOnMap, int indexInArmy)
    {        
        Players.Add(player);
        ++_totalQuantityPlayers;
        player.IndexInMap = indexOnMap;
        player.IndexInList = indexInArmy;
    }

    private void SetPlayerPosition(Player player, int min, int max)
    {
        var position = new Vector3(UnityEngine.Random.Range(min, max),
                                       player.Transform.position.y,
                                       UnityEngine.Random.Range(min, max));

        player.enabled = true;
        player.Transform.position = position;
    }
    
    private IEnumerator SpawnPlayers(int min, int max, List<Player> archers, List<Player> swordman)
    {
        var indexArcher = 0;
        var indexSwordman = 0;
        while (true)
        {
            indexArcher = indexArcher >= SIZE_OF_ARMY ? 0 : indexArcher;
            var playerArcher = archers[indexArcher];          
            SetPlayerPosition(playerArcher, min, max);
            AddPlayer(playerArcher, _index, indexArcher);
            ++indexArcher;
            ++_index;

            indexSwordman = indexSwordman >= SIZE_OF_ARMY ? 0 : indexSwordman;
            var playerSwordman = swordman[indexSwordman];           
            SetPlayerPosition(playerSwordman, min, max);
            AddPlayer(playerSwordman, _index, indexSwordman);
            ++indexSwordman;
            ++_index;

            UpdateQuantityPlayers?.Invoke(_totalQuantityPlayers);

            if (Players.Count >= SIZE_OF_ARMY)
            {
                break;
            }

            yield return new WaitForSeconds(3f);
        }
    }

    private void InitArmy()
    {
        _zombieArchers = new List<Player>();
        _zombieSwordmans = new List<Player>();
        _peopleArchers = new List<Player>();
        _peopleSwordmans = new List<Player>();
    }

    private void CreatedArmy(IFactory<Player> factory, SpecializationType specialization, List<Player> army, int quantity)
    {
        for (var i = 0; i < quantity; ++i)
        {
            var player = factory.Get(specialization);
            player.enabled = false;
            army.Add(player);
        }
    }
}
