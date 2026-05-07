using Assets.Scripts.Game.Maps.Environments;
using Assets.Scripts.Game.PlayerScripts;
using GameFramework.Networking.Movement;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


[RequireComponent (typeof (CharacterController))]
public class PlayerController : NetworkBehaviour
{

    [SerializeField] private float _turnSpeed = 15f;

    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private NetworkMovementComponent _playerMovement;
    [SerializeField] private float _shootRange;
    [SerializeField] private LayerMask _shootingPlayer;

    private CharacterController _characterController;
    private PlayerControl _playerControl;

    private float _cameraAngle;


    private void Start()
    {
        _characterController = GetComponent<CharacterController> ();

        _playerControl = new PlayerControl();
        _playerControl.Enable();

        if(SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Lobby"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned on {OwnerClientId} | IsOwner: {IsOwner} | IsServer: {IsServer}");

        CinemachineCamera cinCamera = _cameraTransform.gameObject.GetComponent <CinemachineCamera> ();

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

        if (IsClient && IsLocalPlayer)
        {
            RotateCamera(lookInput);
            _playerMovement.ProcessLocalPlayerMovement(moveInput, lookInput);

            // Enable this when testing synncing
            /*
            Vector3 movement =
                moveInput.x * transform.right +
                moveInput.y * transform.forward;
            movement.y = 0f;
            _characterController.Move(movement*5f*Time.deltaTime);*/
        }
        else
        {
            _playerMovement.ProcessSimulatedPlayerMovement();
        }

        ShootButton();
        ShootServerRpc();
    }


    private void RotateCamera(Vector2 lookInput)
    {
        _cameraAngle = Vector3.SignedAngle(transform.forward, _cameraTransform.forward, _cameraTransform.right);
        float cameraRotateAmount = lookInput.y * _turnSpeed * Time.deltaTime;
        float newCameraAngle = _cameraAngle - cameraRotateAmount;

        if(newCameraAngle < _minMaxRotationX.x && newCameraAngle > _minMaxRotationX.y)
        {
            _cameraTransform.RotateAround(_cameraTransform.position, _cameraTransform.right, -lookInput.y * Time.deltaTime * _turnSpeed);
        }
    }

    
    private void ShootButton()
    {
        if (IsLocalPlayer && _playerControl.Player.Shoot.inProgress)
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _shootRange, _shootingPlayer))
            {
                if (hit.collider.TryGetComponent<ButtonController>(out ButtonController buttonController))
                {
                    UseButtonServerRpc();
                }

                Debug.Log($"Hit: {hit.ToString()}");
            }
        }
    }


    [ServerRpc]
    private void UseButtonServerRpc()
    {
        if (IsLocalPlayer && _playerControl.Player.Shoot.inProgress)
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _shootRange, _shootingPlayer))
            {
                if (hit.collider.TryGetComponent<ButtonController>(out ButtonController buttonController))
                {
                    buttonController.Activate();
                }
            }
        }
    }


    [ServerRpc]
    private void ShootServerRpc()
    {
        if (IsLocalPlayer && _playerControl.Player.Shoot.inProgress)
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _shootRange, _shootingPlayer))
            {
                if (hit.collider.TryGetComponent<DamageController>(out DamageController damageController))
                {
                    damageController.TakeDamage();
                }
            }
        }
    }

    private void OnDamageTaken()
    {
        Debug.Log("Hit Player");
    }

}