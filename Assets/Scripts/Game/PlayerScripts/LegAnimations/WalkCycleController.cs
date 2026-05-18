using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class WalkCycleController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private List<LegController> _legs;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private LayerMask _groundedMask;

    private float _verticalVelocity;
    private float _gravity = -25f;
    private float _moveSpeed = 5f;

    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //MoveWithInput();

        HandleGravity();

        HandleMovement();

    }


    private void HandleMovement()
    {
        Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>().normalized;

        _characterController.Move(new Vector3(moveInput.x, 0, moveInput.y) * _moveSpeed *Time.deltaTime);

        return;
    }


    private void HandleGravity()
    {
        bool grounded = CheckGrounded();

        if (!grounded)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = 0f;
        }

        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);

        return;
    }

    private void MoveWithInput()
    {
        // This stuff moves the desired position up to the spherical barrier

        _moveInput = _playerControl.Player.Move.ReadValue<Vector2>();

        Vector3 moveOffset = new Vector3(_moveInput.x, _moveInput.y, 0f) * 0.15f;

        foreach(LegController leg in _legs)
        {
            leg.SetFootPosition(moveOffset);
        }

    }

    private bool CheckGrounded()
    {
        RaycastHit hit;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        if (Physics.SphereCast(rayOrigin, 0.2f, -Vector3.up, out hit, 0.2f, _groundedMask))
        {
            Debug.Log("Character is grounded");
            return true;
        }
        return false;
    }
}
