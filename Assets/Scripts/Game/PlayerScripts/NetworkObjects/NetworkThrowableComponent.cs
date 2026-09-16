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

        [SerializeField] private int _tick = 0; public int tick { get { return _tick; } set { _tick = value; } }
        private float _tickRate = 1f / 60f; // This is 60fps
        private float _tickDeltaTime = 0f;


        private const int BUFFER_SIZE = 1024;
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        // Latest transform on the server
        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        public TransformState previousTransformState;


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
            if (IsServer)
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
                Debug.Log("Correcting throwable client position");

                Reconcile(serverState);
            }

            previousTransformState = serverState;
        }


        private void Reconcile(TransformState serverState)
        {
            int bufferIndex = serverState.tick % BUFFER_SIZE;

            TeleportObject(serverState);

            //_transformStates[bufferIndex] = serverState;

            //int replayTick = serverState.tick + 1;
            //int currentTick = _tick;

            //while (replayTick < currentTick)
            //{
            //    bufferIndex = replayTick % BUFFER_SIZE;

            //    TransformState currentState = _transformStates[bufferIndex];

            //    if (currentState.tick != replayTick)
            //    {
            //        break;
            //    }

            //    MoveReplayed(currentState.velocity);

            //    TransformState replayedState =
            //        new TransformState()
            //        {
            //            tick = replayTick,
            //            position = transform.position,
            //            velocity = currentState.velocity,
            //        };

            //    _transformStates[bufferIndex] = replayedState;

            //    replayTick++;
            //}
        }


        private void TeleportObject(TransformState state)
        {
            transform.position = state.position;
            _velocity = state.velocity;
            _tick = state.tick;

            // Reset state in array of states

            int bufferIndex = state.tick % BUFFER_SIZE;
            _transformStates[bufferIndex] = state;
        }

        void Update()
        {
            _tickDeltaTime += Time.deltaTime;

            while (_tickDeltaTime >= _tickRate)
            {
                _tickDeltaTime -= _tickRate;

                SimulateTick();
            }

            try
            {
                Debug.Log($"Servertick {serverTransformState.Value.tick} | Clienttick {_tick}");
            }
            catch (Exception e)
            {

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
                velocity = _velocity
            };

            if (IsServer)
            {
                serverTransformState.Value = transformState;
            }


            if (Time.time > _timer + _lifespan && _timer != 0)
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
            }

            this.transform.position += _velocity * _tickRate;
            _velocity *= _resistance;
        }

        private void MoveReplayed(Vector3 velocity)
        {
            this.transform.position += velocity * _tickRate;
            _velocity *= _resistance;
        }
    }
}

