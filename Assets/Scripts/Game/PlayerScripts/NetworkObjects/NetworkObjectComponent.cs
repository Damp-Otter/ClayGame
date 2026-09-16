using System;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkObjectComponent : NetworkBehaviour
{
    public NetworkVariable<float> gravity = new();
    public NetworkVariable<float> lifespan = new();
    public NetworkVariable<Vector3> scale = new();
    private float _timer;

    [SerializeField] public ObjectDamageController damageController;
    [SerializeField] private Transform _transform;

    public event Action<Vector3> OnObjectDestroyed;


    public override void OnNetworkSpawn()
    {
        _transform.localScale = scale.Value;
        _timer = Time.time;

        if (damageController != null)
        {
            damageController.ResetHealth();

            damageController.OnObjectDestroyed += (pos) =>
            {
                DestroyObject();
            };
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (Time.time > _timer + lifespan.Value)
            {
                DestroyObject();
            }
        }
    }


    public void DestroyObject()
    {
        Debug.Log("Destroyed getting to networkcomponent");

        OnObjectDestroyed?.Invoke(transform.position);

        GetComponent<NetworkObject>().Despawn(true);
    }

}
