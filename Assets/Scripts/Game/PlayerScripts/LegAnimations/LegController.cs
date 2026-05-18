using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LegController : MonoBehaviour
{
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _movePosition;
    [SerializeField] private GameObject _centre;
    [SerializeField] private GameObject _origin;
    [SerializeField] private float _boneLength = 4f;
    [SerializeField] LayerMask _groundedMask;

    [SerializeField] private GameObject _base;

    private Vector3 _previousPosition;
    private bool _isGrounded;


    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }


    private void Update()
    {
        bool _isGrounded = CheckGrounded();

        if (_isGrounded)
        {
            SetToPreviousPosition(_previousPosition);

            return;
        }

        _previousPosition = _movePosition.transform.position;
    }

    public void SetFootPosition(Vector3 offset)
    {
        MoveFootToPosition(offset);

        return;
    }

    private void MoveFootToPosition(Vector3 offset)
    {
        Vector3 worldOffset = _origin.transform.up * offset.x + -_origin.transform.forward * offset.y;

        Vector3 desiredPosition = _movePosition.transform.position + worldOffset;

        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength * _boneLength)
        {
            _movePosition.transform.position = desiredPosition;
        }
        else
        {
            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            _movePosition.transform.position = _centre.transform.position + direction * _boneLength;
        }

        return;
    }

    private void SetToPreviousPosition(Vector3 previousPosition)
    {
        Debug.Log("Setting to previous position");

        _movePosition.transform.position = previousPosition;

        return;
    }

    private bool CheckGrounded()
    {
        RaycastHit hit;

        Vector3 rayOrigin = new Vector3(_movePosition.transform.position.x, _base.transform.position.y + 0.1f, _movePosition.transform.position.z);

        Debug.DrawRay(rayOrigin, _movePosition.transform.forward, Color.green);

        if (Physics.SphereCast(rayOrigin, 0.2f, _movePosition.transform.forward, out hit, 0.3f, _groundedMask))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);

            Debug.Log(hit.transform.gameObject.layer);

            return true;
        }
        return false;
    }
}
