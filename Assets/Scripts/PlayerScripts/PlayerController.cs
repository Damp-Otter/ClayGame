using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    //private PlayerInputController inputController;
    private float _speed = 15f;
    private Vector2 _input = Vector2.zero;

    private void Awake()
    {
        //inputController = GetComponent<PlayerInputController>();
    }


    private void Update()
    {
        if (IsOwner)
        {
            Vector3 positionChange = new Vector3(
                _input.x,
                0,
                _input.y);

            transform.position += positionChange * Time.deltaTime * _speed;

        }
    }


    private void OnMove(InputValue inputValue)
    {
        if (IsOwner)
        {
            _input = inputValue.Get<Vector2>().normalized;
        }

    }

}
