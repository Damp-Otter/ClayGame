using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class WalkCycleController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private List<LegController> _legs;

    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        MoveWithInput();
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
}
