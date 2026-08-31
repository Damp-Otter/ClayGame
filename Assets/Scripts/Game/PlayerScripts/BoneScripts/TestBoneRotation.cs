using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class TestBoneRotation : MonoBehaviour
{

    [SerializeField] private Transform _4;
    [SerializeField] private Transform _3;
    [SerializeField] private Transform _2;
    [SerializeField] private Transform _1;

    [SerializeField] float twistAmount = 4f;
    [SerializeField] float pitchAmount = 2f;

    void Update()
    {
        float twist = 0f;
        float pitch = 0f;

        if (Keyboard.current.aKey.isPressed)
            twist = -twistAmount;

        if (Keyboard.current.dKey.isPressed)
            twist = twistAmount;

        if (Keyboard.current.sKey.isPressed)
            pitch = -pitchAmount;

        if (Keyboard.current.wKey.isPressed)
            pitch = pitchAmount;

        if (Keyboard.current.aKey.isPressed)
            Debug.Log("A pressed");

        if (Keyboard.current.dKey.isPressed)
            Debug.Log("D pressed");

        _4.localRotation = Quaternion.Euler(pitch, 0f, twist);
        _3.localRotation = Quaternion.Euler(pitch, 0f, twist);
        _2.localRotation = Quaternion.Euler(pitch, 0f, twist);
        _1.localRotation = Quaternion.Euler(pitch, 0f, twist);
    }
}
