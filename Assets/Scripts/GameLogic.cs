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
    private const float PPERIOD_FOR_UPDATE = 0.8f;
    private const int SIZE_OF_ARMY = 200;
    
    [SerializeField] private PeopleFactory _peopleFactory;
    [SerializeField] private ZombiFactory _zombiFactory;
    private GameField _gameField;

    private int _index;
    private List<Vector3> pathVector3;

    private List<Player> _peopleSwordmans;
    private List<Player> _peopleArchers;

    private List<Player> _zombieSwordmans;
    private List<Player> _zombieArchers;
    public Dictionary<int, Player> Players { get; private set; }
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

        Players = new Dictionary<int, Player>();

        var max = _gameField.SizeBoard.x <= _gameField.SizeBoard.y ? _gameField.SizeBoard.y :
                                                                     _gameField.SizeBoard.x;

        StartCoroutine(SpawnPlayers(0, max - 1, _peopleArchers, _peopleSwordmans));
        StartCoroutine(SpawnPlayers(0, max - 1, _zombieArchers, _zombieSwordmans));
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        foreach (var player in Players.ToArray())
        {
            if (!player.Value.IsTakeDamage)
            {
                if (!player.Value.IndexMoveToEnemy.HasValue)
                {
                    CreatePathForNearbyEnemy(player.Value);
                    continue;
                }
                if (!Players.ContainsKey(player.Value.IndexMoveToEnemy.Value))
                    continue;              

                if (!Path.ContainsKey(player.Value) || 
                    Path[player.Value] == null)
                    continue;

                var distance = (player.Value.Transform.position - Path[player.Value].Current).sqrMagnitude;
                
                if (!IsComeAcross(player.Value, Players[player.Value.IndexMoveToEnemy.Value]))
                {
                    player.Value.Transform.position = Vector3.MoveTowards(player.Value.Transform.position,
                                                                    Path[player.Value].Current,
                                                                    player.Value.MoveSpeed * deltaTime);
                    if (distance < MAX_DISTANCE)
                    {
                        CreatePathForNearbyEnemy(player.Value);
                        if (Path[player.Value] == null)
                            continue;

                        Path[player.Value].MoveNext();
                    }
                    else
                    {
                        player.Value.TimeUntilNextUpdate += deltaTime;
                        if (player.Value.TimeUntilNextUpdate > PPERIOD_FOR_UPDATE)
                        {
                            player.Value.TimeUntilNextUpdate = 0;
                            CreatePathForNearbyEnemy(player.Value);
                        }
                    }
                }
                else
                {
                    player.Value.IsTakeDamage = true;
                    StartCoroutine(player.Value.TakeDamage.Damage());
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
    public void CreatePathesForNearbyEnemy(Dictionary<int, Player> players)
    {
        float distance;
        float tempararyDistance;

        foreach(var player in players)
        {
            player.Value.IndexMoveToEnemy = null;
            distance = float.MaxValue;

            foreach (var playerJ in Players)
            {
                if (player.Value.RaceType == playerJ.Value.RaceType)
                    continue;

                tempararyDistance = (player.Value.Transform.position - playerJ.Value.Transform.position).magnitude;
                if (distance > tempararyDistance)
                {
                    distance = tempararyDistance;
                    player.Value.IndexMoveToEnemy = playerJ.Value.IndexInDictionary;
                }
            }

            if (Path.ContainsKey(player.Value))
                ChangePath(player.Value); 
            else
                AddPath(player.Value);
        }
    }
    public void SearchPathForNearbyEnemy(Player player)
    {
        float distance;
        float tempararyDistance;

        player.IndexMoveToEnemy = null;
        distance = float.MaxValue;

        foreach (var item in Players)
        {
            if (player.RaceType == item.Value.RaceType)
                continue;

            tempararyDistance = (player.Transform.position - item.Value.Transform.position).magnitude;
            if (distance > tempararyDistance)
            {
                distance = tempararyDistance;
                player.IndexMoveToEnemy = item.Value.IndexInDictionary;
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

    private Dictionary<int, Player> GetPlayrsWithoutTarget(Player takesDamage)
    {
        return Players.Where(player => player.Value.IndexMoveToEnemy == takesDamage.IndexInDictionary)
                      .ToDictionary(k => k.Key, v => v.Value);
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

        if (player.JumpPointParam == null)
            player.JumpPointParam = new JumpPointParam(_gameField.BaseGrid, startPosition, endPosition);
        else
            player.JumpPointParam.Reset(startPosition, endPosition);

        return JumpPointFinder.FindPath(player.JumpPointParam);
    } 

    private void DestoyPlayer(Player takesDamage)
    {
        Path.Remove(takesDamage);
        var playersWitoutTarget = GetPlayrsWithoutTarget(takesDamage);
        Players.Remove(takesDamage.IndexInDictionary);
        DisatcivatePlayer(takesDamage);
        CreatePathesForNearbyEnemy(playersWitoutTarget);

        UpdateQuantityPlayers?.Invoke(Players.Count);
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
        disactivate.Transform.position = new Vector3(-100, -100, -100);
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
        Players.Add(indexOnMap, player);
        player.IndexInDictionary = indexOnMap;
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
            var playerArcher = archers[indexArcher];          
            SetPlayerPosition(playerArcher, min, max);
            AddPlayer(playerArcher, _index, indexArcher);
            ++indexArcher;
            ++_index;

            var playerSwordman = swordman[indexSwordman];           
            SetPlayerPosition(playerSwordman, min, max);
            AddPlayer(playerSwordman, _index, indexSwordman);
            ++indexSwordman;
            ++_index;

            UpdateQuantityPlayers?.Invoke(Players.Count);

            if(indexArcher >= SIZE_OF_ARMY || indexSwordman >= SIZE_OF_ARMY)
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
