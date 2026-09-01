using System;
using UnityEngine;

public class SpineAnimations : MonoBehaviour
{
    private PlayerControl _playerControl;

    [SerializeField] private Transform _4;
    [SerializeField] private Transform _3;
    [SerializeField] private Transform _2;
    [SerializeField] private Transform _1;

    private float _twistAmount = 1f;
    private float _pitchAmount = 0.5f;
    private float _twistMax = 3f;
    private float _pitchMax = 10f;

    private Vector2 _currentDeform;
    private Vector2 _lookInput; public Vector2 lookInput { get { return _lookInput; }  set { _lookInput = value; } }

    void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();

        _currentDeform = new Vector2();
    }

    void Update()
    {
        _currentDeform = CalculateDeform(_currentDeform, lookInput);

        UpdateSpine(_currentDeform);
    }

    private Vector2 CalculateDeform(Vector2 currentDeform, Vector2 lookInput)
    {
        currentDeform.y += -lookInput.y * _pitchAmount * Time.deltaTime;
        currentDeform.x += lookInput.x * _twistAmount * Time.deltaTime;

        currentDeform.y = Mathf.Clamp(currentDeform.y, -_pitchMax, _pitchMax);
        currentDeform.x = Mathf.Clamp(currentDeform.x, -_twistMax, _twistMax);

        return currentDeform;
    }

    private void UpdateSpine(Vector2 deform)
    {
        float twist = deform.x;
        float pitch = deform.y;

        _4.localRotation = Quaternion.Euler(0f, 0, pitch);
        _3.localRotation = Quaternion.Euler(0f, 0, pitch);
        _2.localRotation = Quaternion.Euler(0f, 0, pitch);
        _1.localRotation = Quaternion.Euler(0f, 0, pitch);
    }
}
