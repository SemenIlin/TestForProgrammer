using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const float MAX_DISTANCE = 0.01f;

    private GameLogic _gameLogic;
    private Player _player;
    private void Start()
    {
        _player = GetComponent<Player>();
        _gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>();
    }

    private void Update()
    {
        if (!_player.IndexMoveToEnemy.HasValue)
        {
            _gameLogic.SearchPathForNearbyEnemy(_player);
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
            _gameLogic.TakeDamage(_player, _gameLogic.Players[_player.IndexMoveToEnemy.Value]);
        }
    }
}
