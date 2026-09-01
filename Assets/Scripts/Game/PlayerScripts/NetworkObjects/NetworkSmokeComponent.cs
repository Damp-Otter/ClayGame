using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkSmokeComponent : NetworkBehaviour
{

    private float _timer;
    private float _lifespan; public float lifespan { set { _lifespan = value; } }
    private float _scale; public float scale { set { _scale = value; } }

    [SerializeField] private Transform _transform;

    public event Action<Vector3> OnSmokeDestroyed;

    public override void OnNetworkSpawn()
    {
        _transform.localScale = new Vector3(_scale, _scale, _scale);
        _timer = Time.time;
    }

    void Update()
    {
        if (IsServer)
        {
            if (Time.time > _timer + _lifespan)
            {
                DestroySmoke();
            }
        }
    }

    private void DestroySmoke()
    {
        OnSmokeDestroyed?.Invoke(transform.position);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
