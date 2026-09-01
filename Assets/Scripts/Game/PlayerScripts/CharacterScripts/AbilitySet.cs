using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AbilitySet : NetworkBehaviour
{

    [SerializeField] private GameObject throwablePrefab;
    private Action<Vector3> _onThrowableDestroyed;

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
        ThrowServerRpc(position, direction, gravity, velocity, elasticity, lifespan);

        _onThrowableDestroyed = onThrowableDestroyed;
    }

    [ServerRpc]
    private void ThrowServerRpc(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float lifespan)
    {
        var instance = Instantiate(throwablePrefab);

        NetworkThrowableComponent throwable = instance.GetComponent<NetworkThrowableComponent>();

        throwable.OnThrowableDestroyed += (pos) =>
        {
            _onThrowableDestroyed?.Invoke(pos);
        };

        throwable.lifespan = lifespan;
        throwable.velocity = velocity * direction;
        throwable.gravity = gravity;
        throwable.elasticity = elasticity;

        instance.transform.position = position + direction * 1;
        instance.transform.forward = direction;

        instance.GetComponent<NetworkObject>().Spawn();
    }


    // ----------------------------------------------------------------
    // Smokes
    // ----------------------------------------------------------------


    protected void Smoke(Vector3 position, float lifespan, float scale)
    {
        SmokeServerRpc(position, lifespan, scale);
    }


    [ServerRpc]
    private void SmokeServerRpc(Vector3 position, float lifespan, float scale)
    {
        var instance = Instantiate(smokePrefab);

        NetworkSmokeComponent smoke = instance.GetComponent<NetworkSmokeComponent>();

        smoke.OnSmokeDestroyed += (pos) =>
        {
            _onSmokeDestroyed?.Invoke(pos);
        };

        smoke.lifespan = lifespan;
        smoke.scale = scale;

        instance.transform.position = position;

        Debug.Log("Spawn smoke");
        instance.GetComponent<NetworkObject>().Spawn();
    }


}
