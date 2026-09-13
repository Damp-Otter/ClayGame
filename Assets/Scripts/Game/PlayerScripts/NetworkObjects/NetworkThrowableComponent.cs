using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Game.PlayerScripts.NetworkObjects
{
    public class NetworkThrowableComponent : NetworkBehaviour
    {

        //-------------------------------------------------------------------------------------------
        // In game variables
        //-------------------------------------------------------------------------------------------
        private float _gravity; public float gravity { set { _gravity = value; } }
        private Vector3 _velocity; public Vector3 velocity { set { _velocity = value; } }
        private float _elasticity; public float elasticity { set { _elasticity = value; } }
        private float _resistance; public float resistance { set { _resistance = value; } }


        private float _timer;
        private bool _grounded;
        private float _lastBounceTime = -1f;
        private float _lifespan = 1f; public float lifespan { set { _lifespan = value; } }

        [SerializeField] private Transform _transform;

        public event Action<Vector3> OnThrowableDestroyed;


        //-------------------------------------------------------------------------------------------
        // Networking variables
        //-------------------------------------------------------------------------------------------

        [SerializeField] private int _tick = 0;
        private float _tickRate = 1f / 60f; // This is 60fps
        private float _tickDeltaTime = 0f;


        private const int BUFFER_SIZE = 1024;
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        // Latest transform on the server
        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;

        public override void OnNetworkSpawn()
        {
            _timer = Time.time;
        }

        private void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }
        private void OnDisable()
        {
            serverTransformState.OnValueChanged -= OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousState, TransformState serverState)
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            int bufferIndex = serverState.tick % BUFFER_SIZE;
            TransformState calculatedState = _transformStates[bufferIndex];

            if (calculatedState.tick != serverState.tick)
            {
                return;
            }

            float positionError = Vector3.Distance(calculatedState.position, serverState.position);

            if (positionError > 0.05f)
            {
                Debug.Log("Correcting client position");
            }

            _previousTransformState = serverState;
        }


        void Update()
        {
            Debug.Log($"Velocity {_velocity}");

            _tickDeltaTime += Time.deltaTime;

            while (_tickDeltaTime >= _tickRate)
            {
                _tickDeltaTime -= _tickRate;

                SimulateTick();
            }
        }

        private void SimulateTick()
        {
            int bufferIndex = _tick % BUFFER_SIZE;

            Move();

            TransformState transformState = new TransformState()
            {
                tick = _tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = _velocity
            };

            if (IsServer)
            {
                serverTransformState.Value = transformState;
            }


            if (Time.time > _timer + _lifespan)
            {
                DestroyThrowable();
            }

            _transformStates[bufferIndex] = transformState;

            _tick++;
        }

        public void DestroyThrowable()
        {
            OnThrowableDestroyed?.Invoke(transform.position);

            GetComponent<NetworkObject>().Despawn(true);
        }

        private void Move()
        {
            float radius = _transform.localScale.x / 2;
            float distance = _velocity.magnitude * _tickRate;

            if (Physics.SphereCast(_transform.position + Vector3.up * radius, radius, Vector3.down, out RaycastHit hit, radius))
            {
                Debug.Log("Grounded");
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

            if (Physics.SphereCast(transform.position, radius, _velocity.normalized, out hit, distance))
            {
                transform.position = hit.point + hit.normal * (radius + 0.1f);
                _velocity = Vector3.Reflect(_velocity, hit.normal) * _elasticity;
                _lastBounceTime = Time.time;

                Debug.Log($"BOUNCE tick {_tick} position {transform.position}, is server {IsServer}");
            }
            this.transform.position += _velocity * _tickRate;
            _velocity *= _resistance;
        }

        //[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        //private void MoveRpc(int tick)
        //{
        //    Move();

        //    TransformState transformState = new TransformState()
        //    {
        //        tick = tick,
        //        position = transform.position,
        //        rotation = transform.rotation,
        //        velocity = _velocity
        //    };

        //    serverTransformState.Value = transformState;
        //}
    }
}

