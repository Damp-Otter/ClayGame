using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;


public class ObjectDamageController : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<float> Health = new NetworkVariable<float>();
    [SerializeField] private float _maxHealth;
    public event Action<Vector3> OnObjectDestroyed;


    public void ResetHealth()
    {
        ResetHealthServerRpc();
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ResetHealthServerRpc()
    {
        Health.Value = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer)
            return;

        Health.Value -= damage;

        Health.Value = Mathf.Clamp(Health.Value, 0, _maxHealth);

        if (Health.Value == 0)
        {
            Debug.Log("Destroyed getting to damagecontroller");

            OnObjectDestroyed?.Invoke(transform.position);
        }

    }
}

