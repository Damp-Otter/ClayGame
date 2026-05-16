using UnityEngine;

public class MoveFoot : MonoBehaviour
{
    private Vector2 _moveInput;
    private PlayerControl _playerControl;
    [SerializeField] private GameObject _foot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerControl = new PlayerControl();
        _playerControl.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = _playerControl.Player.Move.ReadValue<Vector2>();

        Vector3 newPostion = new Vector3((_moveInput.x * 0.05f), (_moveInput.y * 0.05f), 0f);

        _foot.transform.position += newPostion;
    }
}
