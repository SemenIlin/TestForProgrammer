using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private const int QUANTITY_BULLETS = 300;
    [SerializeField] private GameObject _peopleBullet;
    [SerializeField] private GameObject _zombieBullet;

    private int indexPeopleBullet;
    private int indexZombieBullet;

    private List<GameObject> _peopleBullets;
    private List<GameObject> _zombieBullets;
    public static BulletPool Instance { get; private set; }
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
        Init();
    }

    public GameObject CreateBullet(RaceType raceType, Vector3 position)
    {
        switch (raceType) 
        {
            case RaceType.zombi:
                if (indexZombieBullet >= QUANTITY_BULLETS)
                    indexZombieBullet = 0;

                var bullet =_zombieBullets[indexZombieBullet];
                bullet.SetActive(true);
                bullet.transform.position = position;
                ++indexZombieBullet;
                return bullet;
            case RaceType.people:
                if (indexPeopleBullet >= QUANTITY_BULLETS)
                    indexPeopleBullet = 0;

                bullet = _peopleBullets[indexPeopleBullet];
                bullet.SetActive(true);
                bullet.transform.position = position;
                ++indexPeopleBullet;
                return bullet;
            default:
                return null;
        }
    }

    public void DestroyBullet(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    private void Init()
    {
        _peopleBullets = new List<GameObject>();
        _zombieBullets = new List<GameObject>();
        for (var i = 0; i < QUANTITY_BULLETS; ++i)
        {
            var zombieBullet = Instantiate(_zombieBullet,transform, false);
            zombieBullet.SetActive(false);

            var peopleBullet = Instantiate(_peopleBullet, transform, false);
            peopleBullet.SetActive(false);

            _peopleBullets.Add(peopleBullet);
            _zombieBullets.Add(zombieBullet);
        }

        indexPeopleBullet = 0;
        indexZombieBullet = 0;
    }
   
}
