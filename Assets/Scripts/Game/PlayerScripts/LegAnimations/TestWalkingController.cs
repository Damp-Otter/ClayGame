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
    private float _moveSpeed = 8f;
    private float _rotationSpeed = 100f;
    private bool _isGrounded;
    public bool justGrounded = false;
    private float _legMaxBoundary = 2.5f;
    private float _legMinBoundary = 0.75f;


    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        bool wasGrounded = _isGrounded;

        _isGrounded = HandleGravity();

        if(_isGrounded && !wasGrounded)
        {
            _controller.HandleLanding();
        }

        HandleMovement();

        HandleTurning();

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


    private bool HandleGravity()
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

        _controller.characterGrounded = grounded;

        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);

        return grounded;
    }


    private bool CheckGrounded()
    {
        RaycastHit hit;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);

        if (Physics.SphereCast(rayOrigin, 0.2f, -Vector3.up, out hit, 0.3f, _groundedMask))
        {
            return true;
        }
        return false;
    }
}
