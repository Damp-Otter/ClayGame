using System;
using UnityEngine;
using UnityEngine.UIElements;

public class JointController : MonoBehaviour
{
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _movePosition;
    [SerializeField] private GameObject _centre; public GameObject centre { get { return _centre; } }
    [SerializeField] private GameObject _origin; public GameObject origin { get { return _origin; } }
    [SerializeField] private float _boneLength = 4f;
    private bool _isStuckToGround; public bool isStuckToGround { get { return _isStuckToGround; } set { _isStuckToGround = value; } }
    private Vector3 _lockedGroundPosition = Vector3.zero;

    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }


    private void Update()
    {
        if (_isStuckToGround)
        {
            if (_lockedGroundPosition != Vector3.zero)
            {
                _movePosition.transform.position = _lockedGroundPosition;
            }

            _lockedGroundPosition = _movePosition.transform.position;
        }
    }

    public void MoveFootByOffest(Vector3 offset)
    {
        Vector3 worldOffset = _origin.transform.up * offset.x + -_origin.transform.forward * offset.y;

        Vector3 desiredPosition = _movePosition.transform.position + worldOffset;

        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            _movePosition.transform.position = desiredPosition;
        }
        else
        {

            Debug.Log("HIT EDGE");

            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            _movePosition.transform.position = _centre.transform.position + direction * _boneLength;
        }

        return;
    }


    public void MoveFootToPosition(Vector3 desiredPosition)
    {
        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            _movePosition.transform.position = desiredPosition;
        }
        else
        {
            Debug.Log("HIT EDGE");

            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            _movePosition.transform.position = _centre.transform.position + direction * _boneLength;
        }

        return;
    }

    public void LockCurrentPosition()
    {
        _lockedGroundPosition = _movePosition.transform.position;
    }

}
