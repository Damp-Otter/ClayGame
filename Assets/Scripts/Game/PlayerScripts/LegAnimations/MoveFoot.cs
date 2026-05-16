using System;
using System.Drawing;
using UnityEngine;

public class MoveFoot : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _desiredJointEnd;
    [SerializeField] private GameObject _thisJoint;

    [SerializeField] private GameObject _parentJoint;
    [SerializeField] private GameObject _parentDesiredJointEnd;

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

        float squaredDistanceToEnd = Vector3.Dot(_desiredJointEnd.transform.position - _thisJoint.transform.position, 
            _desiredJointEnd.transform.position - _thisJoint.transform.position);


        if (squaredDistanceToEnd - 0.3f > _boneLength * _boneLength)
        {
            Debug.Log("Moving forward");

            Vector3 radiusDirection = (_parentDesiredJointEnd.transform.position - _parentJoint.transform.position).normalized;

            Vector3 tangentDirection = Vector3.Cross(radiusDirection, Vector3.forward).normalized;

            Vector3 desiredPosition = _parentDesiredJointEnd.transform.position + tangentDirection * 0.05f;

            Vector3 direction = (desiredPosition - _parentJoint.transform.position).normalized;

            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + direction * _boneLength;

        }
        else if (squaredDistanceToEnd + 0.3f < _boneLength * _boneLength)
        {
            Debug.Log("Moving backward");

            Vector3 radiusDirection = (_parentDesiredJointEnd.transform.position - _parentJoint.transform.position).normalized;

            Vector3 tangentDirection = Vector3.Cross(radiusDirection, -Vector3.forward).normalized;

            Vector3 desiredPosition = _parentDesiredJointEnd.transform.position + tangentDirection * 0.05f;

            Vector3 direction = (desiredPosition - _parentJoint.transform.position).normalized;

            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + direction * _boneLength;
        }
    }
}
