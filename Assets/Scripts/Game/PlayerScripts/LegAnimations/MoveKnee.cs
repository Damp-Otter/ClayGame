using System;
using System.Drawing;
using UnityEngine;

public class MoveKnee : MonoBehaviour
{
    private Vector2 _lookInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _desiredJointEnd;
    [SerializeField] private GameObject _thisJoint;
    [SerializeField] private float _boneLength = 2f;

    [SerializeField] private GameObject _origin;

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

        _rotationAxis = _origin.transform.right;

        Vector3 currentDirection = _thisJoint.transform.up;

        Vector3 targetDirection = (_desiredJointEnd.transform.position - _thisJoint.transform.position).normalized;

        _angle = Vector3.SignedAngle(currentDirection, targetDirection, _rotationAxis);

        if (MathF.Abs(_angle) > 1f)
        {
            _thisJoint.transform.LookAt(_desiredJointEnd.transform);
        }
    }
}
