using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    private float _lastShotTime = 0; public float lastShotTime { get { return _lastShotTime; } set { _lastShotTime = value;  } }
    private float _shotCooldown = 0.3f; public float shotCooldown { get { return _shotCooldown; } set { _shotCooldown = value; } }
    
    public NetworkVariable<float> Health = new NetworkVariable<float>(100);
    private float _maxHealth = 100; public float maxHealth { get { return _maxHealth; } set { _maxHealth = value; } }
    private float _shootRange = 50;  public float shootRange { get { return _shootRange; } set { _shootRange = value; } }
    private float _damage = 10; public float damage { get { return _damage; } set { _damage = value; } }


    private bool _cooledDown = true; public bool cooledDown { get { return _cooledDown; } set { _cooledDown = value; } }

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
