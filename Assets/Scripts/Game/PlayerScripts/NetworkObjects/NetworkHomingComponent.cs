using Assets.Scripts.Game.PlayerScripts.NetworkObjects;
using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkHomingComponent : NetworkThrowableComponent
{
    public event Action<NetworkObjectReference> OnHomingTriggered;
    protected Transform _target; public Transform target { set { _target = value; } }
    protected float _correction; public float correction { set { _correction = value; } }
    protected float _wobble; public float wobble { set { _wobble = value; } }
    protected float _wobbleOffset; public float wobbleOffest { set { _wobbleOffset = value; } }



    protected override void SimulateTick()
    {
        int bufferIndex = _tick % BUFFER_SIZE;

        Move();

        TransformState transformState = new TransformState()
        {
            tick = _tick,
            position = transform.position,
            velocity = _velocity
        };

        if (IsServer)
        {
            serverTransformState.Value = transformState;
        }


        if (Time.time > _timer + _lifespan && _timer != 0 && IsServer)
        {
            SilentDestroyHoming();
        }

        _transformStates[bufferIndex] = transformState;

        _tick++;
    }


    public void TriggerHoming()
    {
        OnHomingTriggered?.Invoke(new NetworkObjectReference(GetComponent<NetworkObject>()));

        GetComponent<NetworkObject>().Despawn(true);
    }


    public void SilentDestroyHoming()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }


    private void Move()
    {
        float radius = meshTransform.localScale.x / 2;
        float distance = _velocity.magnitude * _tickRate;

        if (Physics.SphereCast(meshTransform.position + Vector3.up * radius, radius, Vector3.down, out RaycastHit hit, radius, _layerMask))
        {
            transform.position = hit.point;
            _grounded = true;
        }
        else
        {
            _grounded = false;
        }

        if (!_grounded)
        {
            _velocity.y += _gravity * _tickRate;
        }

        if (Physics.SphereCast(transform.position, radius, _velocity.normalized, out hit, distance, _layerMask))
        {
            transform.position = hit.point + hit.normal * (radius + 0.1f);
            _velocity = Vector3.Reflect(_velocity, hit.normal) * _elasticity;
            _lastBounceTime = Time.time;
        }

        if (_velocity.sqrMagnitude > 0.0001f)
        {
            Vector3 directionToTarget = _target.transform.position - transform.position;

            if (Vector3.Angle(directionToTarget, _velocity) > 30f)
            {
                _velocity = Vector3.RotateTowards(_velocity, directionToTarget, Mathf.Deg2Rad * 100f * _correction * _tickRate, 0f);
            }
        }

        Vector3 positonIncrement = _velocity * _tickRate;

        float angle = (_tick * 0.1f) + _wobbleOffset;
        Vector3 wobbleIncrement = _wobble * Vector3.Cross(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0), _velocity.normalized);

        Vector3 test = Vector3.right;


        this.transform.position += positonIncrement + wobbleIncrement;
        _velocity *= _resistance;
    }
}
