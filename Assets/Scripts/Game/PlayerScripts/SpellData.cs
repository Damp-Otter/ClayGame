using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game.PlayerScripts
{
	public class SpellData: NetworkBehaviour
    {
        [SerializeField] private float _lastShotTime; public float lastShotTime { get { return _lastShotTime; } set { _lastShotTime = value; } }
        [SerializeField] private float _shotCooldown; public float shotCooldown { get { return _shotCooldown; } set { _shotCooldown = value; } }
        [SerializeField] private float _shootRange; public float shootRange { get { return _shootRange; } set { _shootRange = value; } }
        [SerializeField] private float _damage; public float damage { get { return _damage; } set { _damage = value; } }

        public string _spellName = "Basic attack";

        public bool CheckCooldown()
        {
            if (lastShotTime == 0 || _lastShotTime + _shotCooldown <= Time.time)
            {
                Debug.Log("Cooled down");
                lastShotTime = Time.time;
                return true;
            }
            Debug.Log("Waiting for cool down");
            return false;
        }

    }
}