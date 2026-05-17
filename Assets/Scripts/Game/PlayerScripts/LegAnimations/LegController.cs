using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LegController : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _movePosition;
    [SerializeField] private GameObject _centre;
    [SerializeField] private float _boneLength = 4f;

    private float _angle;
    private Vector3 _rotationAxis;


    private void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }


    public void SetFootPosition(Vector3 offset)
    {

        Vector3 worldOffset = transform.up * offset.x + -transform.forward * offset.y;

        Vector3 desiredPosition = _movePosition.transform.position + worldOffset;

        if (Vector3.Dot(_centre.transform.position - desiredPosition, _centre.transform.position - desiredPosition) < _boneLength* _boneLength)
        {
            _movePosition.transform.position = desiredPosition;
        }
        else
        {
            Vector3 direction = (desiredPosition - _centre.transform.position).normalized;

            _movePosition.transform.position = _centre.transform.position + direction * _boneLength;
        }
    }

}
