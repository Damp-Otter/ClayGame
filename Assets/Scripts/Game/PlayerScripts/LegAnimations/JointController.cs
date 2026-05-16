using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class JointController : NetworkBehaviour
{

    [SerializeField] private GameObject _nextJoint;
    [SerializeField] private GameObject _joint;

    private float _angle;
    private float _rotationSpeed = 150f;
    private float _maxStep;

    private float _rotation;
    private Vector2 _moveInput;
    private Vector3 _rotationAxis;


    void Update()
    {
        _rotationAxis = Vector3.right;

        Vector3 currentDirection = _joint.transform.up;

        Vector3 targetDirection = (_nextJoint.transform.position - _joint.transform.position).normalized;

        _angle = Vector3.SignedAngle(currentDirection, targetDirection, _rotationAxis);

        if(MathF.Abs(_angle) > 1f)
        {
            _joint.transform.LookAt(_nextJoint.transform);
        }

    }
}
