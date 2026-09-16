using Assets.Scripts.Game.PlayerScripts.NetworkObjects;
using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkHomingComponent : NetworkThrowableComponent
{
    public event Action<Vector3> OnHomingDestroyed;


    public void DestroyHoming()
    {
        OnHomingDestroyed?.Invoke(transform.position);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
