using Assets.Scripts.Game.Maps.Environments;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static Assets.Scripts.Game.Maps.Environments.ButtonController;

namespace Game
{
	public class DamageController : NetworkBehaviour
	{

        public delegate void DamageTaken(DamageController damageController);
        public event DamageTaken OnDamageTaken;

        public void TakeDamage()
        {
            if (IsServer)
            {
                OnDamageTaken.Invoke(this);
            }
        }
    }
}