using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AbilitySet : NetworkBehaviour
{

    [SerializeField] private GameObject throwablePrefab;
    private Action<Vector3> _onThrowableDestroyed;
    protected NetworkObjectReference throwableReference;

    [SerializeField] private GameObject smokePrefab;
    private Action<Vector3> _onSmokeDestroyed;

    public abstract void AbilityOne();

    public abstract void AbilityTwo();

    public abstract void AbilityThree();


    // ----------------------------------------------------------------
    // Throwables
    // ----------------------------------------------------------------


    protected void Throw(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float lifespan, Action<Vector3> onThrowableDestroyed)
    {
        _onThrowableDestroyed = onThrowableDestroyed;

        ThrowServerRpc(position, direction, gravity, velocity, elasticity, lifespan);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowServerRpc(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float lifespan, RpcParams rpcParams = default)
    {
        var instance = Instantiate(throwablePrefab);

        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        NetworkThrowableComponent throwable = instance.GetComponent<NetworkThrowableComponent>();

        throwable.OnThrowableDestroyed += (pos) =>
        {
            ThrowableDestroyedClientRpc(pos, rpcParams.Receive.SenderClientId);
        };

        throwable.lifespan = lifespan;
        throwable.velocity = velocity * direction;
        throwable.gravity = gravity;
        throwable.elasticity = elasticity;

        instance.transform.position = position + direction * 1;
        instance.transform.forward = direction;

        instance.GetComponent<NetworkObject>().Spawn();

        SetThrowableClientRpc(new NetworkObjectReference(networkObject), rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void SetThrowableClientRpc(NetworkObjectReference reference, ulong targetClientId)
    {
        if (!NetworkManager.Singleton.LocalClientId.Equals(targetClientId))
            return;

        throwableReference = reference;
    }

    [ClientRpc]
    private void ThrowableDestroyedClientRpc(Vector3 position, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        _onThrowableDestroyed?.Invoke(position);
    }

    protected void DestroyThrowable()
    {
        DestroyThrowableServerRpc(throwableReference);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DestroyThrowableServerRpc(NetworkObjectReference reference)
    {
        if (reference.TryGet(out NetworkObject networkObject))
        {
            NetworkThrowableComponent throwable = networkObject.GetComponent<NetworkThrowableComponent>();

            throwable.DestroyThrowable();
        }
    }


    // ----------------------------------------------------------------
    // Smokes
    // ----------------------------------------------------------------


    protected void Smoke(Vector3 position, float lifespan, float scale, float gravity)
    {
        SmokeServerRpc(position, lifespan, scale, gravity);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SmokeServerRpc(Vector3 position, float lifespan, float scale, float gravity, RpcParams rpcParams = default)
    {
        var instance = Instantiate(smokePrefab);

        NetworkSmokeComponent smoke = instance.GetComponent<NetworkSmokeComponent>();

        smoke.OnSmokeDestroyed += (pos) =>
        {
            _onSmokeDestroyed?.Invoke(pos);
        };

        smoke.gravity.Value = gravity;
        smoke.lifespan.Value = lifespan;
        smoke.scale.Value = scale;

        instance.transform.position = position;

        instance.GetComponent<NetworkObject>().Spawn();
    }
}
