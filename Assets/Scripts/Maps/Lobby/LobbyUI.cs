using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI _text;

    private void Start()
    {
        _text.text = $"Lobby code: {GameLobbyManager.singleton.GetLobbyCode()}";
    }

}
