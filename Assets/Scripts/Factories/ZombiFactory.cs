using System;
using UnityEngine;

[CreateAssetMenu]
public class ZombiFactory : GameObjectFactory
{
    [Serializable]
    class ZombiConfig
    {
        public Player Prefab;

        [Range(1f, 100f)]
        public float Damage = 10f;

        [Range(1, 20)]
        public float MoveSpeed = 3f;

        [Range(0f, 100f)]
        public float Distance = 1f;

        [Range(10f, 1000f)]
        public float Health = 100f;
    }

    [SerializeField]
    private ZombiConfig _swoardMan;

    [SerializeField]
    private ZombiConfig _archer;

    public Player Get(SpecializationType specialization)
    {
        var config = GetConfig(specialization);

        var _player = CreateGameObjectInstance(config.Prefab);

        _player.Initialize(config.MoveSpeed,
                            config.Health,
                            config.Damage,
                            config.Distance);

        return _player;
    }

    public void Reclame(Player player)
    {
        Destroy(player.gameObject);
    }

    private ZombiConfig GetConfig(SpecializationType specialization)
    {
        switch (specialization)
        {
            case SpecializationType.swordsman:
                return _swoardMan;
            case SpecializationType.archer:
                return _archer;
        }

        return _swoardMan;
    }
}
