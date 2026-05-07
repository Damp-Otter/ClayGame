using Assets.Scripts.Game.Maps.Environments;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static Assets.Scripts.Game.Maps.Environments.ButtonController;

namespace Game
{
    public class DamageController : NetworkBehaviour
    {
        [SerializeField] private PlayerData _playerData;

        public void TakeDamage(float damage)
        {
            if (!IsServer)
                return;

            _playerData.Health.Value -= damage;

            _playerData.Health.Value = Mathf.Clamp(_playerData.Health.Value, 0, _playerData.maxHealth);

        }
    }
}