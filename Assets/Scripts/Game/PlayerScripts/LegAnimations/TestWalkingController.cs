using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class TestWalkingController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private LayerMask _groundedMask;
    [SerializeField] private WalkCycle _controller;

    private float _verticalVelocity;
    private float _gravity = -25f;
    private float _jumpHeight = 8f;

    private float _moveSpeed = 8f;
    private float _rotationSpeed = 150f;


    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();

        _controller.jumpHeight = _jumpHeight;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        HandleTurning();

        HandleGravityAndJumping();

    }


    private void HandleMovement()
    {

        Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();

        Vector3 movement = moveInput.x * transform.right + moveInput.y * transform.forward;

        movement.Normalize();

        _characterController.Move(movement * _moveSpeed * Time.deltaTime);

        if (moveInput != Vector2.zero)
        {
            _controller.isMoving = true;
        }
        else
        {
            _controller.isMoving = false;
        }

        HandleGravityAndJumping();

        return;
    }


    private void HandleTurning()
    {
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>().normalized;

        transform.Rotate(Vector3.up, lookInput.x * _rotationSpeed * Time.deltaTime);

        if (lookInput != Vector2.zero)
        {
            _controller.isTurning = true;
            _controller.turnDirection = Convert.ToInt32(lookInput.x < 0);
        }
        else
        {
            _controller.isTurning = false;
        }

        return;

    }


    private void HandleGravityAndJumping()
    {
        bool grounded = CheckGrounded();

        bool jumpInput = _playerControl.Player.Jump.triggered;

        if (jumpInput && grounded)
        {
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _controller.HandleJumping();
        }
        else if (!grounded)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = 0f;
        }

        if (grounded && _controller.characterGrounded == false)
        {
            _controller.HandleLanding();
        }

        _controller.characterGrounded = grounded;
        _controller.verticalVelocity = _verticalVelocity;

        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);

        return;
    }


    private bool CheckGrounded()
    {
        RaycastHit hit;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        if (Physics.SphereCast(rayOrigin, 0.2f, -Vector3.up, out hit, 0.5f, _groundedMask))
        {
            return true;
        }
        return false;
    }
}
