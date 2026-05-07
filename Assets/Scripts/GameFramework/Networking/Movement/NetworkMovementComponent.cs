using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Networking.Movement
{
    public class NetworkMovementComponent : NetworkBehaviour
    {

        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _turnSpeed = 15f;
        [SerializeField] private Vector2 _minMaxRotationX;

        [SerializeField] private GameObject _camera;
        [SerializeField] private Transform _cameraTransform;
        private float _cameraPitch;

        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Color _color;

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerControl _playerControl;

        [SerializeField] private int _tick = 0;
        private float _tickRate = 1f / 60f; // This is 60fps
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        // Latest transform on the server
        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;

        private void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }
        private void OnDisable()
        {
            serverTransformState.OnValueChanged -= OnServerStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _cameraTransform = _camera.transform;
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

            float positionError =
            Vector3.Distance(calculatedState.position, serverState.position);

            if (positionError > 0.001f)
            {

                Debug.Log("Correcting client position");

                // Out of sync

                Reconcile(serverState);

            }

            _previousTransformState = serverState;
        }

        private void Reconcile(TransformState serverState)
        {
            int bufferIndex = serverState.tick % BUFFER_SIZE;

            TeleportPlayer(serverState);

            _transformStates[bufferIndex] = serverState;

            int replayTick = serverState.tick + 1;
            int currentTick = _tick;

            while (replayTick < currentTick)
            {
                bufferIndex = replayTick % BUFFER_SIZE;

                InputState inputState =
                    _inputStates[bufferIndex];

                if (inputState.tick != replayTick)
                {
                    break;
                }

                RotatePlayer(inputState.lookInput);
                MovePlayer(inputState.movementInput);

                TransformState replayedState =
                    new TransformState()
                    {
                        tick = replayTick,
                        position = transform.position,
                        rotation = transform.rotation,
                        hasStartedMoving = true
                    };

                _transformStates[bufferIndex] =
                    replayedState;

                replayTick++;
            }
        }

        private void TeleportPlayer(TransformState state)
        {
            _characterController.enabled = false;
            _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            transform.position = state.position;
            transform.rotation = state.rotation;
            _characterController.enabled = true;

            // Reset state in array of states

            int bufferIndex = state.tick % BUFFER_SIZE;
            _transformStates[bufferIndex] = state;
        }


        public void ProcessLocalPlayerMovement(Vector2 moveInput, Vector2 lookInput)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex = _tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRpc(_tick, moveInput, lookInput);
                    RotatePlayer(lookInput);
                    MovePlayer(moveInput);
                }
                else
                {
                    RotatePlayer(lookInput);
                    MovePlayer(moveInput);

                    TransformState state = new TransformState()
                    {
                        tick = _tick,
                        position = transform.position,
                        rotation = transform.rotation,
                        hasStartedMoving = true
                    };

                    _previousTransformState = serverTransformState.Value;
                    serverTransformState.Value = state;
                }

                InputState inputState = new InputState()
                {
                    tick = _tick,
                    movementInput = moveInput,
                    lookInput = lookInput
                };

                TransformState transformState = new TransformState()
                {
                    tick = _tick,
                    position = transform.position,
                    rotation = transform.rotation,
                    hasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }


        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (serverTransformState.Value.hasStartedMoving)
                {
                    transform.position = serverTransformState.Value.position;
                    transform.rotation = serverTransformState.Value.rotation;

                    _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
                }

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }


        private void MovePlayer(Vector2 movementInput)
        {
            Vector3 movement =
                movementInput.x * transform.right +
                movementInput.y * transform.forward;
            movement.y = 0f;

            // Gravity is here!? Seems odd but okay
            if (!_characterController.isGrounded)
            {
                movement.y = Physics.gravity.y;
            }

            _characterController.Move(movement * _tickRate * _speed);
        }


        private void RotatePlayer(Vector2 lookInput)
        {
            transform.Rotate(
                Vector3.up,
                lookInput.x * _tickRate * _turnSpeed);
        }


        [ServerRpc]
        private void MovePlayerServerRpc(int tick, Vector2 moveInput, Vector2 lookInput)
        {
            RotatePlayer(lookInput);
            MovePlayer(moveInput);

            TransformState state = new TransformState()
            {
                tick = tick,
                position = transform.position,
                rotation = transform.rotation,
                hasStartedMoving = true
            };

            _previousTransformState = serverTransformState.Value;
            serverTransformState.Value = state;

        }


        protected virtual void OnDrawGizmos()
        {

            if (serverTransformState.Value.hasStartedMoving)
            {
                Gizmos.color = _color;
                Gizmos.DrawMesh(_meshFilter.mesh, serverTransformState.Value.position);
            }
        }

    }
}
