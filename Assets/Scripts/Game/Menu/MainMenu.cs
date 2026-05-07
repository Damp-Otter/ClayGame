using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Authentication and matchmaking stuff will eventually happen, 
/// </summary>
namespace Game
{
    public class MainMenu : MonoBehaviour
    {

        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private GameObject _joinScreen;
        [SerializeField] private Button _hostButton = null;
        [SerializeField] private Button _clientButton = null;

        [SerializeField] private Button _submitCodeButton;
        [SerializeField] private TextMeshProUGUI _codeText;

        private void OnEnable()
        {

#if !UNITY_EDITOR && UNITY_SERVER
        OnServerClicked();
        return
#endif


            _hostButton.onClick.AddListener(OnHostClicked);
            _clientButton.onClick.AddListener(OnClientClicked);
            _submitCodeButton.onClick.AddListener(OnJoinClicked);

        }


        private void OnDisable()
        {
            _hostButton.onClick.RemoveListener(OnHostClicked);
            _clientButton.onClick.RemoveListener(OnClientClicked);
            _submitCodeButton.onClick.RemoveListener(OnJoinClicked);
        }


        private async void OnHostClicked()
        {

            bool succeeded = await GameLobbyManager.singleton.CreateLobby();
            if (succeeded)
            {
                await SceneManager.LoadSceneAsync("Lobby");
            }

        }


        private void OnClientClicked()
        {
            _mainScreen.SetActive(false);
            _joinScreen.SetActive(true);
        }

        private async void OnJoinClicked()
        {
            string code = _codeText.text;
            code = code.Substring(0, code.Length - 1);
            Debug.Log(code);

            bool succeeded = await GameLobbyManager.singleton.JoinLobby(code);
            Debug.Log(succeeded);
            if (succeeded)
            {
                await SceneManager.LoadSceneAsync("Lobby");
            }
        }

    }

}
