using System;
using System.Drawing;
using UnityEngine;

public class MoveFoot : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _desiredJointEnd;
    [SerializeField] private GameObject _thisJoint;
    [SerializeField] private float _boneLength = 2f;

    private float _angle;
    private Vector3 _rotationAxis;


    void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    void Update()
    {
        // This rotates the ball joint to look at the desired position

        _rotationAxis = Vector3.right;

        Vector3 currentDirection = _thisJoint.transform.up;

        Vector3 targetDirection = (_desiredJointEnd.transform.position - _thisJoint.transform.position).normalized;

        _angle = Vector3.SignedAngle(currentDirection, targetDirection, _rotationAxis);

        if (MathF.Abs(_angle) > 1f)
        {
            _thisJoint.transform.LookAt(_desiredJointEnd.transform);
        }

        // This stuff moves the desired position across the spherical plane


        float distanceToEnd = Vector3.Dot(_desiredJointEnd.transform.position - _thisJoint.transform.position, 
            _desiredJointEnd.transform.position - _thisJoint.transform.position);

        Debug.Log($"Distance to end: {distanceToEnd}");

        if (distanceToEnd + 0.3f < _boneLength * _boneLength)
        {
            Debug.Log("Moving forward");

            Vector3 moveOffset = new Vector3(1, 1, 0f) * 0.05f;

            Vector3 desiredPosition = _thisJoint.transform.position + moveOffset;

            Vector3 direction = (desiredPosition - _desiredJointEnd.transform.position).normalized;

            _thisJoint.transform.position = _desiredJointEnd.transform.position + direction * _boneLength;
        }
        else if (distanceToEnd - 0.3f > _boneLength * _boneLength)
        {
            Debug.Log("Moving backward");

            Vector3 moveOffset = new Vector3(1, 1, 0f) * -0.05f;

            Vector3 desiredPosition = _thisJoint.transform.position + moveOffset;

            Vector3 direction = (desiredPosition - _desiredJointEnd.transform.position).normalized;

            _thisJoint.transform.position = _desiredJointEnd.transform.position + direction * _boneLength;
        }
    }
}
