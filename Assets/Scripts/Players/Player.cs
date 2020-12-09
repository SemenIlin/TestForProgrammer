using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    [SerializeField] private SpecializationType _specialization;
    [SerializeField] private RaceType _raceType;
    public Player() { }
    public Player(float moveSpeed, float health, float damage, float distance)
    {
        MoveSpeed = moveSpeed;
        Health = health;
        Damage = damage;
        Distance = distance;
    }
    public SpecializationType SpecializationType => _specialization;
    public RaceType RaceType => _raceType;

    /// <summary>
    /// It is position in List people.
    /// </summary>
    public int IndexMoveToEnemy { get; set; }
    public List<Vector3> PathVector { get; set; } = new List<Vector3>();
    public Transform Transform { get; set; }
    public float Magnitude { get; set; }
    public float MoveSpeed { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
    public float Distance { get; set; }
    public void Initialize(float moveSpeed, float health, float damage, float distance)
    {
        MoveSpeed += moveSpeed;
        Health += health;
        Damage += damage;
        Distance += distance;
    }

    public static Player operator +(Player a, Player b)
    {
        return new Player()
        {
            Damage = a.Damage + b.Damage,
            Health = a.Health + b.Health,
            MoveSpeed = a.MoveSpeed + b.MoveSpeed,
            Distance = a.Distance + b.Distance
        };
    }
    public static Player operator *(Player a, float b)
    {
        return new Player()
        {
            Damage = a.Damage * b,
            Health = a.Health * b,
            MoveSpeed = a.MoveSpeed * b,
            Distance = a.Distance * b
        };
    }
}

