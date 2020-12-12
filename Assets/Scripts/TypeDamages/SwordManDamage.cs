using System.Collections;
using UnityEngine;

public class SwordManDamage : MonoBehaviour, ITakeDamage
{
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
        _gameLogic.TakeDamage(_player, _gameLogic.Players[_player.IndexMoveToEnemy.Value]);
        yield return new WaitForSeconds(GameLogic.INTERVAL / _player.AttackSpeed);
        _player.IsTakeDamage = false;
    }
}
