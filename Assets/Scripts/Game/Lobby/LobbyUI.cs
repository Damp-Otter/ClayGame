using Game.Events;
using GameFramework.Data;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
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
        [SerializeField] private Button _mapUpButton;
        [SerializeField] private Button _mapDownButton;
        [SerializeField] private MapSelectionData _mapSelectionData;
        private int _currentMapIndex = 0;

        [SerializeField] private TextMeshProUGUI _characterName;
        [SerializeField] private Button _characterUpButton;
        [SerializeField] private Button _characterDownButton;
        [SerializeField] private CharacterSelectionData _characterSelectionData;
        private int _currentCharacterIndex = 0;

        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _startText;

        private bool _waitingForLobbySync = false;


        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
            LobbyEvents.OnLobbyReady += OnLobbyReady;

            _readyButton.onClick.AddListener(OnReadyPressed);

            _characterUpButton.onClick.AddListener(OnCharacterUpPressed);
            _characterDownButton.onClick.AddListener(OnCharacterDownPressed);

            if (GameLobbyManager.singleton.isHost)
            {
                _startButton.onClick.AddListener(OnStartPressed);

                _mapUpButton.onClick.AddListener(OnMapUpPressed);
                _mapDownButton.onClick.AddListener(OnMapDownPressed);
            }
        }


        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
            LobbyEvents.OnLobbyReady -= OnLobbyReady;

            _readyButton.onClick.RemoveAllListeners();
            _mapUpButton.onClick.RemoveAllListeners();
            _mapDownButton.onClick.RemoveAllListeners();
            _startButton.onClick.RemoveAllListeners();

        }


        private async void Start()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.singleton.GetLobbyCode()}";

            _startButton.image.color = Color.softRed;
            _startText.fontSize = 50;
            _startText.text = "WAITING FOR PLAYERS";

            await GameLobbyManager.singleton.SetSelectedMap(_currentMapIndex, _mapSelectionData.maps[_currentMapIndex].sceneName);

            if (!GameLobbyManager.singleton.isHost)
            {
                _mapUpButton.gameObject.SetActive(false);
                _mapDownButton.gameObject.SetActive(false);
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
            _startButton.enabled = false;

            await GameLobbyManager.singleton.StartGame();
        }


        private async void OnMapDownPressed()
        {
            _currentMapIndex--;
            if (_currentMapIndex < 0)
            {
                _currentMapIndex = _mapSelectionData.maps.Count - 1;
            }

            UpdateMap();
            _waitingForLobbySync = true;

            await GameLobbyManager.singleton.SetSelectedMap(_currentMapIndex, _mapSelectionData.maps[_currentMapIndex].sceneName);
        }


        private async void OnMapUpPressed()
        {
            _currentMapIndex++;
            if (_currentMapIndex > _mapSelectionData.maps.Count - 1)
            {
                _currentMapIndex = 0;
            }

            UpdateMap();
            _waitingForLobbySync = true;

            await GameLobbyManager.singleton.SetSelectedMap(_currentMapIndex, _mapSelectionData.maps[_currentMapIndex].sceneName);
        }


        private async void OnCharacterDownPressed()
        {
            _currentCharacterIndex--;
            if (_currentCharacterIndex < 0)
            {
                _currentCharacterIndex = _characterSelectionData.characters.Count - 1;
            }

            UpdateCharacter();
            _waitingForLobbySync = true;

            await GameLobbyManager.singleton.SetSelectedCharacter(_currentCharacterIndex, _characterSelectionData.characters[_currentCharacterIndex].name);
        }


        private async void OnCharacterUpPressed()
        {
            _currentCharacterIndex++;
            if (_currentCharacterIndex > _characterSelectionData.characters.Count - 1)
            {
                _currentCharacterIndex = 0;
            }

            UpdateCharacter();
            _waitingForLobbySync = true;

            await GameLobbyManager.singleton.SetSelectedCharacter(_currentCharacterIndex, _characterSelectionData.characters[_currentCharacterIndex].name);
        }


        private void UpdateMap()
        {
            _mapName.text = _mapSelectionData.maps[_currentMapIndex].sceneName;
        }

        private void UpdateCharacter()
        {
            _characterName.text = _characterSelectionData.characters[_currentCharacterIndex].name;
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