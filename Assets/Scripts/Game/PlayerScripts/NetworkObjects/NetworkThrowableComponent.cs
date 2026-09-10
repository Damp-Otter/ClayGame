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
        private float _lifespan; public float lifespan { set { _lifespan = value; } }

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

                // Out of sync

                //Reconcile(serverState);

            }

            _previousTransformState = serverState;
        }


        void Update()
        {
            int bufferIndex = _tick % BUFFER_SIZE;

            if (!IsServer)
            {
                MoveRpc();
                BounceRpc();

                if (Time.time > _timer + _lifespan)
                {
                    DestroyThrowable();
                }
            }
            else
            {
                Move();
                Bounce();
            }

            _tick++;

            TransformState transformState = new TransformState()
            {
                tick = _tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = _velocity
            };

            _transformStates[bufferIndex] = transformState;
        }

        public void DestroyThrowable()
        {
            OnThrowableDestroyed?.Invoke(transform.position);

            GetComponent<NetworkObject>().Despawn(true);
        }

        private void Move()
        {
            if (!_grounded) {
                _velocity.y += _gravity * _tickRate;
            }
            else
            {
                _velocity.y = 0;
            }

            this.transform.position += _velocity * _tickRate;
            _velocity *= _resistance;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void MoveRpc()
        {
            Move();
        }


        private void Bounce()
        {
            float radius = _transform.localScale.x / 2;
            float distance = _velocity.magnitude * Time.deltaTime;

            if (Physics.SphereCast(transform.position, radius, _velocity.normalized, out RaycastHit hit, distance))
            {

                if (!_grounded)
                {
                    _grounded = CheckGrounded();
                }

                _velocity = Vector3.Reflect(_velocity, hit.normal) * _elasticity;
                _lastBounceTime = Time.time;
            }
        }

        private bool CheckGrounded()
        {
            float radius = _transform.localScale.x / 2;

            Debug.Log($"Time betweeen {Time.time - _lastBounceTime}, Time {Time.time}, last bounce {_lastBounceTime}");

            if (Physics.SphereCast(transform.position, radius, Vector3.down, out RaycastHit hit, radius) && Time.time - _lastBounceTime < 0.15f)
            {
                Debug.Log("Throwable grounded");
                return true;
            }
            return false;
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void BounceRpc()
        {
            Bounce();
        }

    }
}

