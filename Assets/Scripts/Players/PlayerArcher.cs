using System.Collections;
using UnityEngine;

public class PlayerArcher : MonoBehaviour
{
    private const float MAX_DISTANCE = 0.01f;
    private const float INTERVAL = 5f;

    [SerializeField] private GameObject _bullet;

    private GameLogic _gameLogic;
    private Player _player;
    private bool _isTakeDamage;
    private void Start()
    {
        _player = GetComponent<Player>();
        _gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>();
        _isTakeDamage = false;
    }

    private void Update()
    {
        if (!_isTakeDamage)
        {
            if (!_player.IndexMoveToEnemy.HasValue)
            {
                _gameLogic.SearchPathForNearbyEnemy(_player);
                return;
            }
            if (_player.PathVector.Count == 0)
            {
                _gameLogic.CreatePathForNearbyEnemy(_player);
                return;
            }

            var distanceI = (_player.Transform.position - _gameLogic.Path[_player].Current).sqrMagnitude;

            if (!_gameLogic.IsComeAcross(_player, _gameLogic.Players[_player.IndexMoveToEnemy.Value]))
            {
                _player.Transform.position = Vector3.MoveTowards(_player.Transform.position,
                                                                 _gameLogic.Path[_player].Current,
                                                                 _player.MoveSpeed * Time.deltaTime);
                if (distanceI < MAX_DISTANCE)
                {
                    _gameLogic.RemovePath(_player);
                    _gameLogic.CreatePathForNearbyEnemy(_player);
                    _gameLogic.Path[_player].MoveNext();
                }
            }
            else
            {
                _isTakeDamage = true;
                StartCoroutine(TakeDamage());
            }
        }
    }

    private IEnumerator TakeDamage()
    {
        var bullet = Instantiate(_bullet, _player.Transform, false);
        var bulletTransform = bullet.GetComponent<Transform>();
        bulletTransform.position = _player.Transform.position;
        var playerTakeDamage = _gameLogic.Players[_player.IndexMoveToEnemy.Value];

        float t = 0;
        while (t < 1)
        {
            if (playerTakeDamage.Health <= 0)
                break;

            bulletTransform.position = Vector3.Lerp(bulletTransform.position, playerTakeDamage.Transform.position, t);
            t += Time.deltaTime / (INTERVAL / _player.AttackSpeed);
            yield return null;
        }

        Destroy(bullet);
        if (playerTakeDamage.Health > 0)
            _gameLogic.TakeDamage(_player, playerTakeDamage);
       
        yield return new WaitForSeconds(INTERVAL / _player.AttackSpeed);
        _isTakeDamage = false;
    }
}
