using Game.Events;
using GameFramework.Data;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] private Button _readyButton;

        [SerializeField] private TextMeshProUGUI _mapName;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private MapSelectionData _mapSelectionData;
        private int _currentMapIndex = 0;

        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _startText;

        private bool _waitingForLobbySync = false;


        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
            LobbyEvents.OnLobbyReady += OnLobbyReady;

            _readyButton.onClick.AddListener(OnReadyPressed);

            if (GameLobbyManager.singleton.isHost)
            {
                _startButton.onClick.AddListener(OnStartPressed);
                _upButton.onClick.AddListener(OnUpPressed);
                _downButton.onClick.AddListener(OnDownPressed);
            }
        }


        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
            LobbyEvents.OnLobbyReady -= OnLobbyReady;

            _readyButton.onClick.RemoveAllListeners();
            _upButton.onClick.RemoveAllListeners();
            _downButton.onClick.RemoveAllListeners();
            _startButton.onClick.RemoveAllListeners();

        }


        private void Start()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.singleton.GetLobbyCode()}";

            _startButton.image.color = Color.softRed;
            _startText.fontSize = 50;
            _startText.text = "WAITING FOR PLAYERS";

            if (!GameLobbyManager.singleton.isHost)
            {
                _upButton.gameObject.SetActive(false);
                _downButton.gameObject.SetActive(false);
            }

        }


        private async void OnReadyPressed()
        {
            bool succeeded = await GameLobbyManager.singleton.SetPlayerReady();

            if (succeeded)
            {
                _readyButton.gameObject.SetActive(false);
            }
        }


        private async void OnStartPressed()
        {
            await GameLobbyManager.singleton.StartGame(_mapSelectionData.maps[_currentMapIndex].sceneName);
        }


        private async void OnDownPressed()
        {
            _currentMapIndex--;
            if (_currentMapIndex < 0)
            {
                _currentMapIndex = _mapSelectionData.maps.Count - 1;
            }

            UpdateMap();
            _waitingForLobbySync = true;

            await GameLobbyManager.singleton.SetSelectedMap(_currentMapIndex);
        }


        private async void OnUpPressed()
        {
            _currentMapIndex++;
            if (_currentMapIndex > _mapSelectionData.maps.Count - 1)
            {
                _currentMapIndex = 0;
            }

            UpdateMap();

            await GameLobbyManager.singleton.SetSelectedMap(_currentMapIndex);
        }


        private void UpdateMap()
        {
            _mapName.text = _mapSelectionData.maps[_currentMapIndex].mapName;
        }

        private void OnLobbyUpdated()
        {
            int lobbyMapIndex = GameLobbyManager.singleton.GetMapIndex();

            var players = GameLobbyManager.singleton.GetPlayers();
            bool localPlayerReady = false;

            int numberOfReadyPlayers = 0;

            foreach (var player in players)
            {
                if(player.id == AuthenticationService.Instance.PlayerId)
                {
                    localPlayerReady = player.isReady;
                }
                if (player.isReady)
                {
                    numberOfReadyPlayers++;
                }
            }

            if (_waitingForLobbySync)
            {
                if (lobbyMapIndex == _currentMapIndex)
                {
                    _waitingForLobbySync = false;
                }
                return;
            }

            if (lobbyMapIndex != _currentMapIndex)
            {
                _currentMapIndex = lobbyMapIndex;
                UpdateMap();
            }


            if (numberOfReadyPlayers == players.Count && !GameLobbyManager.singleton.isHost)
            {
                _startButton.image.color = Color.paleGreen;
                _startText.fontSize = 80;
                _startText.text = "READY";
            }

        }

        private void OnLobbyReady()
        {
            _startButton.image.color = Color.paleGreen;
            _startText.fontSize = 80;
            _startText.text = "START";
        }

    }

}