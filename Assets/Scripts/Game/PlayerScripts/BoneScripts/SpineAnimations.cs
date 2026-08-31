using System;
using UnityEngine;

public class SpineAnimations : MonoBehaviour
{
    private PlayerControl _playerControl;

    [SerializeField] private Transform _4;
    [SerializeField] private Transform _3;
    [SerializeField] private Transform _2;
    [SerializeField] private Transform _1;

    private float _twistAmount = 100f;
    private float _pitchAmount = 50f;
    private float _twistMax = 10f;
    private float _pitchMax = 10f;

    private Vector2 _currentDeform;

    void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();

        _currentDeform = new Vector2();
    }

    void Update()
    {
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>().normalized;

        _currentDeform = CalculateDeform(_currentDeform, lookInput);
        
        _currentDeform.y += -lookInput.y * _pitchAmount * Time.deltaTime;
        _currentDeform.x += lookInput.x * _twistAmount * Time.deltaTime;

        _currentDeform.y = Mathf.Clamp(_currentDeform.y, -_pitchMax, _pitchMax);
        _currentDeform.x = Mathf.Clamp(_currentDeform.x, -_twistMax, _twistMax);

        UpdateSpine(_currentDeform);
    }

    private Vector2 CalculateDeform(Vector2 currentDeform, Vector2 lookInput)
    {
        currentDeform.y += -lookInput.y * _pitchAmount * Time.deltaTime;
        currentDeform.x += lookInput.x * _twistAmount * Time.deltaTime;

        currentDeform.y = Mathf.Clamp(_currentDeform.y, -_pitchMax, _pitchMax);
        currentDeform.x = Mathf.Clamp(_currentDeform.x, -_twistMax, _twistMax);

        return currentDeform;
    }

    private void UpdateSpine(Vector2 deform)
    {
        float twist = deform.x;
        float pitch = deform.y;

        Debug.Log($"Twist: {twist}, Pitch: {pitch}");

        _4.localRotation = Quaternion.Euler(0f, twist, pitch);
        _3.localRotation = Quaternion.Euler(0f, twist, pitch);
        _2.localRotation = Quaternion.Euler(0f, twist, pitch);
        _1.localRotation = Quaternion.Euler(0f, twist, pitch);
    }
}
