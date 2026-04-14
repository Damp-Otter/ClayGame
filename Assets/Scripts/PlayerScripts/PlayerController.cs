using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    //private PlayerInputController inputController;
    private float speed = 15f;
    private NetworkVariable<Vector2> _input = new NetworkVariable<Vector2>(Vector2.zero,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        //inputController = GetComponent<PlayerInputController>();
    }


    private void Update()
    {
        if (IsServer)
        {
            Vector3 positionChange = new Vector3(
                _input.Value.x,
                0,
                _input.Value.y);

            transform.position += positionChange * Time.deltaTime * speed;
        }

    }

    private void OnMove(InputValue inputValue)
    {
        if (IsOwner)
        {
            _input.Value = inputValue.Get<Vector2>().normalized;
        }

    }

}
