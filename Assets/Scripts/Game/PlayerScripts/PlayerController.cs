using Assets.Scripts.Game.Maps.Environments;
using Assets.Scripts.Game;
using GameFramework.Networking.Movement;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Game;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {

        [SerializeField] private float _turnSpeed = 5f;

        [SerializeField] private PlayerData _playerData;

        [SerializeField] private Vector2 _minMaxRotationX;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private NetworkMovementComponent _playerMovement;
        [SerializeField] private LayerMask _shootingPlayer;

        private CharacterController _characterController;
        private PlayerControl _playerControl;
        [SerializeField] private DamageController _damageController;
        [SerializeField] private PlayerAnimationController _playerAnimationController;

        private float _cameraAngle;


        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _damageController = GetComponent<DamageController>();

            _playerControl = new PlayerControl();
            _playerControl.Enable();

            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Lobby"))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }


        public override void OnNetworkSpawn()
        {
            Debug.Log($"Spawned on {OwnerClientId} | IsOwner: {IsOwner} | IsServer: {IsServer}");

            CinemachineCamera cinCamera = _cameraTransform.gameObject.GetComponent<CinemachineCamera>();

            if (IsOwner)
            {
                cinCamera.Priority = 1;
            }
            else
            {
                cinCamera.Priority = 0;
            }

        }


        private void Update()
        { 
            Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();
            Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();

            if (!_playerData.jumping && _playerControl.Player.Jump.inProgress)
            {
                Debug.Log("Jumping from player controller");
                _playerData.jumping = true;
            }

            if (IsClient && IsLocalPlayer)
            {
                RotateCamera(lookInput);

                _playerMovement.ProcessLocalPlayerMovement(moveInput, lookInput, _playerData.jumping);
            }
            else
            {
                _playerMovement.ProcessSimulatedPlayerMovement();
            }

            if (IsLocalPlayer && _playerControl.Player.Shoot.inProgress)
            {
                _playerData.cooledDown = _playerData.CheckCooldown();

                if (_playerData.cooledDown)
                {
                    ShootButton(_cameraTransform.position, _cameraTransform.forward);
                    ShootServerRpc(_cameraTransform.position, _cameraTransform.forward);
                }
            }
        }


        private void RotateCamera(Vector2 lookInput)
        {
            _cameraAngle = Vector3.SignedAngle(transform.forward, _cameraTransform.forward, _cameraTransform.right);
            float cameraRotateAmount = lookInput.y * _turnSpeed * Time.deltaTime;
            float newCameraAngle = _cameraAngle - cameraRotateAmount;

            if (newCameraAngle < _minMaxRotationX.x && newCameraAngle > _minMaxRotationX.y)
            {
                _cameraTransform.RotateAround(_cameraTransform.position, _cameraTransform.right, -lookInput.y * Time.deltaTime * _turnSpeed);
            }
        }


        private void ShootButton(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _playerData.shootRange, _shootingPlayer))
            {
                if (hit.collider.TryGetComponent<ButtonController>(out ButtonController buttonController))
                {
                    UseButtonServerRpc(origin, direction);
                }
            }
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void UseButtonServerRpc(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, _playerData.shootRange, _shootingPlayer))
            {
                if (hit.collider.TryGetComponent<ButtonController>(out ButtonController buttonController))
                {
                    buttonController.Activate();
                }
            } 
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ShootServerRpc(Vector3 origin, Vector3 direction)
        {
            if (!_playerData.cooledDown)
                return;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _playerData.shootRange))
            {
                DamageController damageController =
                    hit.collider.GetComponentInParent<DamageController>();

                if (damageController != null)
                {
                    damageController.TakeDamage(_playerData.damage);
                }
            }
        }
    }
}