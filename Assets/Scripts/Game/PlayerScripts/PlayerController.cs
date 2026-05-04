using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;
using System;
using UnityEngine.SceneManagement;


[RequireComponent (typeof (CharacterController))]
public class PlayerController : NetworkBehaviour
{

    //private PlayerInputController inputController;
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _turnSpeed = 15f;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _characterController;
    private PlayerControl _playerControl;

    private float _cameraAngle;

    private Vector2 moveInput = Vector2.zero;


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


    private void Move(Vector2 movementInput)
    {
        Vector3 movement = moveInput.x * _cameraTransform.right + moveInput.y * _cameraTransform.forward;
        movement.y = 0f;

        _characterController.Move(movement * Time.deltaTime * _speed);
    }


    private void Rotate(Vector2 lookInput)
    {
        transform.RotateAround(transform.position, transform.up, lookInput.x * Time.deltaTime * _turnSpeed);
    }


    private void RotateCamera(float lookInputY)
    {
        _cameraAngle = Vector3.SignedAngle(transform.forward, _cameraTransform.forward, _cameraTransform.right);
        float cameraRotationAmount = lookInputY * Time.deltaTime * _turnSpeed;
        float newCameraAngle = _cameraAngle - cameraRotationAmount;
        if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
        {
            _cameraTransform.RotateAround(_cameraTransform.position, _cameraTransform.right, -lookInputY * Time.deltaTime * _turnSpeed);
        }
    }


    private void Update()
    {
        if (IsOwner)
        {

            if (_playerControl.Player.Move.inProgress)
            {
                Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();
                Move(moveInput);
            }
            if (_playerControl.Player.Look.inProgress)
            {
                Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();
                Rotate(lookInput);
            }
        }
    }

    [ServerRpc]
    private void MoveServerRPC(Vector2 moveInput, Vector2 lookInput)
    {
        Move(moveInput);
        Rotate(lookInput);
    }

}

/*using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;
using System;
using UnityEngine.SceneManagement;


[RequireComponent (typeof (CharacterController))]
public class PlayerController : NetworkBehaviour
{

    //private PlayerInputController inputController;
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _turnSpeed = 15f;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _characterController;
    private PlayerControl _playerControl;

    private float _cameraAngle;

    private Vector2 moveInput = Vector2.zero;


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

    private void Move(Vector2 movementInput)
    {
        Vector3 movement = moveInput.x * _cameraTransform.right + moveInput.y * _cameraTransform.forward;
        movement.y = 0f;

        _characterController.Move(movement * Time.deltaTime * _speed);
    }


    private void Rotate(Vector2 lookInput)
    {
        transform.RotateAround(transform.position, transform.up, lookInput.x * Time.deltaTime * _turnSpeed);
    }


    private void RotateCamera(float lookInputY)
    {
        _cameraAngle = Vector3.SignedAngle(transform.forward, _cameraTransform.forward, _cameraTransform.right);
        float cameraRotationAmount = lookInputY * Time.deltaTime * _turnSpeed;
        float newCameraAngle = _cameraAngle - cameraRotationAmount;
        if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
        {
            _cameraTransform.RotateAround(_cameraTransform.position, _cameraTransform.right, -lookInputY * Time.deltaTime * _turnSpeed);
        }
    }


    private void Update()
    {
        Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();

        if (IsLocalPlayer && IsServer)
        {
            RotateCamera(lookInput.y);
            Move(moveInput);
            Rotate(lookInput);
        }
        else if (IsLocalPlayer)
        {
            RotateCamera(lookInput.y);
            MoveServerRPC(moveInput, lookInput);
        }
    }


    [ServerRpc]
    private void MoveServerRPC(Vector2 moveInput, Vector2 lookInput)
    {
        Move(moveInput);
        Rotate(lookInput);
    }
}
*/