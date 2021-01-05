using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    [SerializeField] private SpecializationType _specialization;
    [SerializeField] private RaceType _raceType;
    public SpecializationType SpecializationType => _specialization;
    public RaceType RaceType => _raceType;

    /// <summary>
    /// It is position in List people.
    /// </summary>
    public int? IndexMoveToEnemy { get; set; }
    /// <summary>
    /// Global index player on map.
    /// </summary>
    public int IndexInMap { get; set; }

    /// <summary>
    /// Index in List of army.
    /// </summary>
    public int IndexInList { get; set; }
    public float TimeUntilNextUpdate { get; set; }
    public List<Vector3> PathVector { get; set; } = new List<Vector3>();
    public Transform Transform { get; set; }
    public float AttackSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
    public float Distance { get; set; }
    public bool IsTakeDamage { get; set; }
    public ITakeDamage TakeDamage { get; set; }

    public void Initialize(float moveSpeed, float health, float damage, float distance, float attackSpeed)
    {
        MoveSpeed = moveSpeed;
        Health = health;
        Damage = damage;
        Distance = distance;
        AttackSpeed = attackSpeed;
        IsTakeDamage = false;
    }
}

