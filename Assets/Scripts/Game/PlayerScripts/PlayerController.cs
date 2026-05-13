using Assets.Scripts.Game;
using Assets.Scripts.Game.Maps.Environments;
using Game;
using GameFramework.Networking.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {

        private float _lookSensitivity = 1f;

        [SerializeField] private PlayerData _playerData;

        [SerializeField] private Vector2 _minMaxRotationX;
        [SerializeField] private Transform _cameraTransform;
        private float _pitch;

        [SerializeField] private NetworkMovementComponent _playerMovement;
        [SerializeField] private LayerMask _shootingPlayer;


        private CharacterController _characterController;
        private PlayerControl _playerControl;
        [SerializeField] private DamageController _damageController;
        [SerializeField] private PlayerAnimationController _playerAnimationController;

        private float _cameraAngle;

        private bool _isRespawning = false;


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

            Camera camera = _cameraTransform.GetComponentInChildren<Camera>();
            AudioListener audioListener = _cameraTransform.GetComponentInChildren<AudioListener>();

            if (IsOwner)
            {
                camera.enabled = true;
                audioListener.enabled = true;
            }
            else
            {
                camera.enabled = false;
                audioListener.enabled = false;
            }

        }


        private void Update()
        {
            if (!IsOwner)
                return;

            Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();
            Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();

            bool jump = HandleJumping();

            HandleMovement(moveInput, lookInput, jump);

            HandleShoot();

        }

        private void FixedUpdate()
        {
            if (IsServer && !_playerData.isAlive.Value && !_isRespawning)
            {
                HandleDeath();
            }
        }


        private void HandleDeath()
        {
            Transform spawnPoint = SpawnPoints.singleton.GetPointInOrder();

            CharacterController controller = GetComponent<CharacterController>();
            controller.enabled = false;

            _characterController.enabled = false;
            transform.position = spawnPoint.position;
            _characterController.enabled = true;

            controller.enabled = true;

            _playerData.Health.Value = _playerData.characterData.maxHealth;
            _playerData.isAlive.Value = true;

            _playerAnimationController.UpdateHealthBar();

            _isRespawning = false;
        }


        private void HandleShoot()
        {
            if (IsLocalPlayer && _playerControl.Player.Shoot.inProgress)
            {
                _playerData.cooledDown = _playerData.spellData.CheckCooldown();

                if (_playerData.cooledDown)
                {
                    ShootButton(_cameraTransform.position, _cameraTransform.forward);
                    ShootServerRpc(_cameraTransform.position, _cameraTransform.forward);
                }
            }
        }


        private void HandleMovement(Vector2 moveInput, Vector2 lookInput, bool jumpPressed)
        {
            if (IsClient && IsLocalPlayer)
            {
                RotateCamera(lookInput);

                _playerMovement.ProcessLocalPlayerMovement(moveInput, lookInput, jumpPressed);
            }
            else
            {
                _playerMovement.ProcessSimulatedPlayerMovement();
            }
        }


        private bool HandleJumping()
        {
            if (_playerControl.Player.Jump.inProgress && _playerData.isGrounded)
            {
                return true;
            }
            return false;
        }


        private void RotateCamera(Vector2 lookInput)
        {
            _pitch -= lookInput.y * _lookSensitivity * _playerData.senstivityMultiplier;

            _pitch = Mathf.Clamp(
                _pitch,
                _minMaxRotationX.x,
                _minMaxRotationX.y);

            _cameraTransform.localRotation =
                Quaternion.Euler(_pitch, 0f, 0f);
        }


        private void ShootButton(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _playerData.spellData.shootRange, _shootingPlayer))
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
            if (Physics.Raycast(origin, direction, out RaycastHit hit, _playerData.spellData.shootRange, _shootingPlayer))
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

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _playerData.spellData.shootRange))
            {
                DamageController damageController =
                    hit.collider.GetComponentInParent<DamageController>();

                if (damageController != null && damageController != _damageController)
                {
                    damageController.TakeDamage(_playerData.spellData.damage);
                }
            }
        }
    }
}