using System;
using UnityEngine;

public class LegController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _movePosition;
    [SerializeField] private GameObject _centre;
    [SerializeField] private float _boneLength = 4f;

    private float _angle;
    private Vector3 _rotationAxis;


    void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    void Update()
    {
        // This stuff moves the desired position across the spherical plane

        _moveInput = _playerControl.Player.Move.ReadValue<Vector2>();

        Vector3 moveOffset = new Vector3(_moveInput.x, _moveInput.y, 0f) * 0.05f;

        Vector3 desiredPosition = _movePosition.transform.position + moveOffset;

        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            _movePosition.transform.position += moveOffset;
        }

    }
}
