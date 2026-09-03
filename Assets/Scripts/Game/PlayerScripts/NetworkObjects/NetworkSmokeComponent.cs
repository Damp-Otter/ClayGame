using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkSmokeComponent : NetworkBehaviour
{
    public NetworkVariable<float> gravity = new();
    public NetworkVariable<float> lifespan = new();
    public NetworkVariable<float> scale = new();
    private Vector3 _velocity; public Vector3 velocity { set { _velocity = value; } }

    private bool landed = false;

    private float _timer;

    [SerializeField] private Transform _transform;

    public event Action<Vector3> OnSmokeDestroyed;

    public override void OnNetworkSpawn()
    {
        _transform.localScale = Vector3.one * scale.Value;
        _timer = Time.time;
    }

    void Update()
    {
        if (IsServer) {

            if (!landed)
            {
                Move();
                Land();
            }

            if (Time.time > _timer + lifespan.Value)
            {
                DestroySmoke();
            }
        }

    }

    private void Move()
    {
        _velocity.y += gravity.Value * Time.deltaTime;
        this.transform.position += _velocity * Time.deltaTime;
    }

    private void Land()
    {
        float radius = transform.localScale.x * 1.5f;
        if (Physics.SphereCast(transform.position - _velocity.normalized * radius, radius, _velocity.normalized, out RaycastHit hit, radius))
        {
            landed = true;
            _velocity = Vector3.zero;
        }
    }


    private void DestroySmoke()
    {
        OnSmokeDestroyed?.Invoke(transform.position);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
