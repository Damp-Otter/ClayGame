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

    [SerializeField] private GameObject _origin;

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

        _rotationAxis = _origin.transform.right;

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

        Debug.Log($"SquaredDistanceToEnd: {squaredDistanceToEnd}");

        if (squaredDistanceToEnd - 0.3f > _boneLength * _boneLength)
        { 
            Vector3 radiusDirection = (_parentDesiredJointEnd.transform.position - _parentJoint.transform.position).normalized;

            Vector3 tangentDirection = Vector3.Cross(_origin.transform.right, radiusDirection).normalized;

            Vector3 desiredPosition = _parentDesiredJointEnd.transform.position + tangentDirection * 0.15f;

            Vector3 direction = (desiredPosition - _parentJoint.transform.position).normalized;

            Debug.Log($"End position {_parentJoint.transform.position + direction * _boneLength}");

            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + direction * _boneLength;
        }
        else if (squaredDistanceToEnd + 0.3f < _boneLength * _boneLength)
        {
            Vector3 radiusDirection = (_parentDesiredJointEnd.transform.position - _parentJoint.transform.position).normalized;

            Vector3 tangentDirection = -Vector3.Cross(_origin.transform.right, radiusDirection).normalized;

            if (tangentDirection.x > 0)
            {
                Debug.Log("Moving backward, Going backwards?");
            }

            Vector3 desiredPosition = _parentDesiredJointEnd.transform.position + tangentDirection * 0.15f;

            Vector3 direction = (desiredPosition - _parentJoint.transform.position).normalized;

            Debug.Log($"End position {_parentJoint.transform.position + direction * _boneLength}");

            _parentDesiredJointEnd.transform.position = _parentJoint.transform.position + direction * _boneLength;
        }
    }
}


/*using System;
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
    private float _orbitDirection = 1;


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


        if (squaredDistanceToEnd - 1f > _boneLength * _boneLength || squaredDistanceToEnd + 1f < _boneLength * _boneLength)
        {
            // Current constrained direction on sphere
            Vector3 radiusDirection =
                (_parentDesiredJointEnd.transform.position
                - _parentJoint.transform.position).normalized;

            // Stable tangent direction
            Vector3 tangentDirection =
                Vector3.Cross(Vector3.forward, radiusDirection).normalized;

            // Direction toward unconstrained desired point
            Vector3 toTarget =
                (_desiredJointEnd.transform.position
                - _parentDesiredJointEnd.transform.position).normalized;

            // Check whether tangent moves toward target
            float alignment =
                Vector3.Dot(tangentDirection, toTarget);

            // Flip tangent if moving away from target
            if (alignment < -0.98f)
            {
                _orbitDirection *= -1;
            }

            tangentDirection *= _orbitDirection;

            // Move along sphere
            Vector3 correctedPosition =
                _parentDesiredJointEnd.transform.position
                + tangentDirection * 0.1f;

            // Re-project to sphere
            Vector3 correctedDirection =
                (correctedPosition - _parentJoint.transform.position).normalized;

            _parentDesiredJointEnd.transform.position =
                _parentJoint.transform.position
                + correctedDirection * _boneLength;

        }
    }
}
*/