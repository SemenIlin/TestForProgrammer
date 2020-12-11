using System.Collections;
using UnityEngine;

public class PlayerSwordman : MonoBehaviour
{
    private const float MAX_DISTANCE = 0.01f;
    private const float INTERVAL = 5f;

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
        _gameLogic.TakeDamage(_player, _gameLogic.Players[_player.IndexMoveToEnemy.Value]);
        yield return new WaitForSeconds(INTERVAL / _player.AttackSpeed);
        _isTakeDamage = false;
    }
}
