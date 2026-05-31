using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class MoveFoot : MonoBehaviour {

    private Vector2 _moveInput; 
    private PlayerControl _playerControl; 
    [SerializeField] private GameObject _desiredJointEnd; 
    [SerializeField] private GameObject _thisJoint; 
    [SerializeField] private GameObject _parentJoint; 
    [SerializeField] private GameObject _parentDesiredJointEnd; 
    [SerializeField] private float _boneLength = 2f;

    [SerializeField] private GameObject _origin;
    [SerializeField] private GameObject _centre;

    private float _angle; 
    private Vector3 _rotationAxis; 

    void Start() { 
        _playerControl = new PlayerControl(); 
        _playerControl.Enable(); 
    } 

    
    void Update() 
    {
        // This moves the joint around a circular plane

        float squaredDistanceToEnd = Vector3.Dot(_desiredJointEnd.transform.position - _thisJoint.transform.position, 
            _desiredJointEnd.transform.position - _thisJoint.transform.position);

        Vector3 _desiredParentJointEnd = CalculateDesiredPoint();

        // This direction is going to calculate a plane that runs through the
        // foot and hip, and also up to avoid twsiting :)

        Vector3 toTarget = _desiredJointEnd.transform.position - _parentJoint.transform.position;

        Vector3 planeNormal = Vector3.Cross(toTarget, _origin.transform.up).normalized;

        Vector3 direction = _desiredParentJointEnd - _parentJoint.transform.position;

        direction = Vector3.ProjectOnPlane(direction, planeNormal).normalized;


        // Calculating the signed angles of knee and foot

        float footAngle = CalculateUnisgnedAngle(_desiredJointEnd.transform.position, _centre.transform.position, planeNormal);

        float kneeAngle = CalculateUnisgnedAngle(_desiredParentJointEnd, _centre.transform.position, planeNormal);

        if (!(footAngle > kneeAngle))
        {
            // This rotates the ball joint to look at the desired position

            RotateBallJoint();

            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + direction * _boneLength;
        }
        else
        {
            _parentJoint.transform.LookAt(_desiredJointEnd.transform);
            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + _parentJoint.transform.forward * _boneLength;

            RotateBallJoint();
        }
    }


    private void RotateBallJoint()
    {
        _thisJoint.transform.position = _parentDesiredJointEnd.transform.position;

        _rotationAxis = _origin.transform.right;
        Vector3 currentDirection = _thisJoint.transform.up;
        Vector3 targetDirection = (_desiredJointEnd.transform.position - _thisJoint.transform.position).normalized;
        _angle = Vector3.SignedAngle(currentDirection, targetDirection, _rotationAxis);

        if (MathF.Abs(_angle) > 1f)
        {
            _thisJoint.transform.LookAt(_desiredJointEnd.transform);
        }
    }


    private float CalculateUnisgnedAngle(Vector3 from, Vector3 to, Vector3 plane)
    {
        Vector3 direction = (from - to).normalized;

        float angle = Vector3.SignedAngle(Vector3.up, direction, _origin.transform.right);
        if (angle < 0)
        {
            angle = 360 + angle;
        }

        return angle;
    }

    private Vector3 CalculateDesiredPoint()
    {
        float squaredDistanceToEnd = Vector3.Dot(_desiredJointEnd.transform.position - _thisJoint.transform.position,
            _desiredJointEnd.transform.position - _thisJoint.transform.position);

        Vector3 _desiredParentJointEnd = _parentDesiredJointEnd.transform.position;

        if (squaredDistanceToEnd - 1f > _boneLength * _boneLength)
        {
            Vector3 currentDirection = _thisJoint.transform.forward;
            _desiredParentJointEnd = _parentDesiredJointEnd.transform.position + currentDirection * 0.1f;
        }
        else if (squaredDistanceToEnd + 1f < _boneLength * _boneLength)
        {
            Vector3 currentDirection = -_thisJoint.transform.forward;
            _desiredParentJointEnd = _parentDesiredJointEnd.transform.position + currentDirection * 0.1f;
        }

        return _desiredParentJointEnd;

    }
}
