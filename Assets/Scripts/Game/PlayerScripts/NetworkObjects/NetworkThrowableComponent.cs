using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkThrowableComponent : NetworkBehaviour
{
    private float _gravity; public float gravity { set { _gravity = value; } }
    private Vector3 _velocity; public Vector3 velocity { set { _velocity = value; } }

    private float _elasticity; public float elasticity { set { _elasticity = value; } }


    private float _timer;
    private float _lifespan; public float lifespan { set { _lifespan = value; } }

    [SerializeField] private Transform _transform;

    public event Action<Vector3> OnThrowableDestroyed;

    public override void OnNetworkSpawn()
    {
        _timer = Time.time;
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        MoveRpc();

        BounceRpc();

        if (Time.time > _timer + _lifespan) {
            DestroyThrowable();
        }
        
    }

    public void DestroyThrowable()
    {
        OnThrowableDestroyed?.Invoke(transform.position);

        GetComponent<NetworkObject>().Despawn(true);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void MoveRpc()
    {
        _velocity.y += _gravity * Time.deltaTime;
        this.transform.position += _velocity * Time.deltaTime;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void BounceRpc()
    {
        float radius = _transform.localScale.x / 2;
        if (Physics.SphereCast(transform.position - _velocity.normalized * radius, radius, _velocity.normalized, out RaycastHit hit, radius))
        {
            _velocity = Vector3.Reflect(_velocity, hit.normal) * _elasticity;
        }
    }

}
