using System.Collections;
using UnityEngine;

public class ArcherDamage : MonoBehaviour, ITakeDamage
{
    [SerializeField] private GameObject _bullet;
    private GameLogic _gameLogic;
    private Player _player;
    private void OnEnable()
    {
        _gameLogic = GameLogic.Instance;
        _player = GetComponent<Player>();
        _player.TakeDamage = this;

    }
    public IEnumerator Damage()
    {
        var bullet = Instantiate(_bullet, _player.Transform, false);

        var bulletTransform = bullet.GetComponent<Transform>();
        bulletTransform.position = _player.Transform.position;

        var playerTakeDamage = _gameLogic.Players[_player.IndexMoveToEnemy.Value];
        var deltaTime = Time.deltaTime;

        float t = 0;
        while ((t < 1) && (playerTakeDamage.Health > 0))
        {
            if (_player == null)
            {
                _player.IsTakeDamage = false;
                yield break;
            }

            bulletTransform.position = Vector3.Lerp(bulletTransform.position, playerTakeDamage.Transform.position, t);
            t += deltaTime / (GameLogic.INTERVAL / _player.AttackSpeed);
            yield return null;
        }

        Destroy(bullet);
        if (playerTakeDamage.Health > 0)
        {
            _gameLogic.TakeDamage(_player, playerTakeDamage);
            yield return new WaitForSeconds(GameLogic.INTERVAL / _player.AttackSpeed);
        }

        _player.IsTakeDamage = false;
        yield break;
    }
}
