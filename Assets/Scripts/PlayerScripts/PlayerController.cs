using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    private PlayerInputController inputController;
    private float speed = 15f;
    private NetworkVariable<Vector2> _input = null;

    private void Awake()
    {
        inputController = GetComponent<PlayerInputController>();
        _input = new NetworkVariable<Vector2>(inputController.MovementInputVector,
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner);
    }


    private void Update()
    {

        

        Vector3 positionChange = new Vector3(
            inputController.MovementInputVector.y, 
            0,
            -inputController.MovementInputVector.x);

        transform.position += positionChange * Time.deltaTime * speed;
    }


}
