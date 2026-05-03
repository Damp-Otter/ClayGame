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


            _hostButton.onClick.AddListener(OnServerClicked);
            _clientButton.onClick.AddListener(OnClientClicked);
            _submitCodeButton.onClick.AddListener(OnJoinClicked);

        }


        private void OnDisable()
        {
            _hostButton.onClick.RemoveListener(OnServerClicked);
            _clientButton.onClick.RemoveListener(OnClientClicked);
            _submitCodeButton.onClick.RemoveListener(OnJoinClicked);
        }


        private async void OnServerClicked()
        {

            bool succeeded = await GameLobbyManager.singleton.CreateLobby();
            if (succeeded)
            {
                await SceneManager.LoadSceneAsync("Lobby");
            }
            //ConnectionManager.singleton.InitializeAsServer(5678);
            //SceneManager.LoadScene(2);

        }


        private void OnClientClicked()
        {
            _mainScreen.SetActive(false);
            _joinScreen.SetActive(true);
            //ConnectionManager.singleton.InitializeAsClient("127.0.0.1", 5678);
            //SceneManager.LoadScene(2);

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
