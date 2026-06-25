using System;
using UnityEngine;
using UnityEngine.UIElements;

public class JointController : MonoBehaviour
{
    private PlayerControl _playerControl;
    [SerializeField] public GameObject movePosition;
    [SerializeField] private GameObject _centre; public GameObject centre { get { return _centre; } }
    [SerializeField] private GameObject _origin; public GameObject origin { get { return _origin; } }
    [SerializeField] private float _boneLength = 4f;
    [SerializeField] private Transform _parentTransform;
    private bool _isStuckToGround; public bool isStuckToGround { get { return _isStuckToGround; } set { _isStuckToGround = value; } }
    private Vector3 _lockedGroundPosition = Vector3.zero; public Vector3 lockedGroundPosition { get { return _lockedGroundPosition; } }

    private Quaternion _initialRotation; public Quaternion initialRotation { get { return _initialRotation; } }
    private Quaternion _defaultRotation; public Quaternion defaultRotation { get { return _defaultRotation; } set { _defaultRotation = value; } }



    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();

        _initialRotation = transform.localRotation;
        _boneLength = _boneLength * _parentTransform.localScale.x;
    }


    public void MoveFootByOffset(Vector3 offset)
    {
        Vector3 worldOffset = _origin.transform.up * offset.x + -_origin.transform.forward * offset.y;

        Vector3 desiredPosition = movePosition.transform.position + worldOffset;

        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            movePosition.transform.position = desiredPosition;
        }
        else
        {
            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            movePosition.transform.position = _centre.transform.position + direction * _boneLength;
            
            throw new Exception("HIT EDGE");
        }

        return;
    }


    public bool MoveFootToPosition(Vector3 desiredPosition)
    {
        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            movePosition.transform.position = desiredPosition;
        }
        else
        {
            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            movePosition.transform.position = _centre.transform.position + direction * _boneLength;

            return false;
        }

        return true;
    }


    public void TurnLegByOffset(Vector3 offset)
    {
        _origin.transform.Rotate(offset);
    }


    public void LockCurrentPosition()
    {
        _lockedGroundPosition = movePosition.transform.position;
    }

    public void LockPosition(Vector3 position)
    {
        _lockedGroundPosition = position;
    }

}
