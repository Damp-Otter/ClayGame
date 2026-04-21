using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;
using System;

public class SwitchController : NetworkBehaviour
{

    private NetworkVariable<bool> _isActive = new NetworkVariable<bool>();


    public delegate void SwitchChanged(SwitchController switchController, bool isActive);
    public event SwitchChanged OnSwitchChanged;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _isActive.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(bool wasActive, bool isActive)
    {
        if (isActive)
        {
            Debug.Log("isActive");
        }
        else
        {
            Debug.Log("isNotActive");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnSwitchChangedServerRpc(true);
    }


    private void OnTriggerExit(Collider other)
    {
        OnSwitchChangedServerRpc(false);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void OnSwitchChangedServerRpc(bool isActive)
    {
        _isActive.Value = isActive;
        OnSwitchChanged(this, isActive);
    }

}
