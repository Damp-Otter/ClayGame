using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class WalkingControllerController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private LayerMask _groundedMask;
    [SerializeField] private WalkCycleController _controller;

    private float _verticalVelocity;
    private float _gravity = -25f;
    private float _moveSpeed = 5f;
    private bool _isGrounded;
    public bool justGrounded = false;

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

    }


    private void HandleMovement()
    {

        Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>().normalized;

        _characterController.Move(new Vector3(moveInput.x, 0, moveInput.y) * _moveSpeed * Time.deltaTime);

        if(moveInput != Vector2.zero)
        {
            _controller.isMoving = true;
        }
        else
        {
            _controller.isMoving = false;
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

        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);

        return grounded;
    }


    private bool CheckGrounded()
    {
        RaycastHit hit;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);

        if (Physics.SphereCast(rayOrigin, 0.2f, -Vector3.up, out hit, 0.3f, _groundedMask))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);

            return true;
        }
        return false;
    }
}
