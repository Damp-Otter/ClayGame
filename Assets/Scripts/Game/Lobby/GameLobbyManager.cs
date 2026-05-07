using GameFramework.Data;
using GameFramework.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Game
{
    public class GameLobbyManager : MonoBehaviour
    {
        private static GameLobbyManager _singleton; public static GameLobbyManager singleton { get { return _singleton; } }
        private const int MAX_PLAYERS = 5;

        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();
        private LobbyPlayerData _localLobbyPlayerData;
        private LobbyData _lobbyData;
        private bool _isTransitioning = false;

        public bool isHost => _localLobbyPlayerData.id == LobbyManager.singleton.GetHostId();

        void Start()
        {
            if (_singleton == null)
            {
                _singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }


        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }


        // Changes to make: Server instantly loads the player, then in update lobby, set correct players
        public async Task<bool> CreateLobby()
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

            _lobbyData = new LobbyData();
            _lobbyData.Initialize(0);

            bool succeeded = await LobbyManager.singleton.CreateLobby(MAX_PLAYERS, true, _localLobbyPlayerData.Serialize(), _lobbyData.Serialize());

            return succeeded;
        }


        public virtual string GetLobbyCode()
        {
            return LobbyManager.singleton.GetLobbyCode();
        }


        public async Task<bool> JoinLobby(string code)
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");

            bool succeeded = await LobbyManager.singleton.JoinLobby(code, _localLobbyPlayerData.Serialize());

            return succeeded;
        }

        private async void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.singleton.GetPlayersData();
            _lobbyPlayerDatas.Clear();

            int numberOfReadyPlayers = 0;
            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.UpdateState(data);

                if (lobbyPlayerData.isReady)
                {
                    numberOfReadyPlayers++;
                }

                if (lobbyPlayerData.id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayerDatas.Add(lobbyPlayerData);
            }

            _lobbyData = new LobbyData();
            _lobbyData.UpdateState(lobby.Data);

            Events.LobbyEvents.OnLobbyUpdated?.Invoke();

            if (numberOfReadyPlayers == lobby.Players.Count && isHost)
            {
                Events.LobbyEvents.OnLobbyReady?.Invoke();
            }

            if(_lobbyData.joinRelayCode != default && !_isTransitioning && !isHost)
            {
                _isTransitioning = true;

                LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
                LobbyManager.singleton.StopLobbyUpdates();

                await JoinRelayServer(_lobbyData.joinRelayCode);
                await SceneManager.LoadSceneAsync(_lobbyData.sceneName);
            }

        }

        internal List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayerDatas;
        }

        internal async Task<bool> SetPlayerReady()
        {
            _localLobbyPlayerData.isReady = true;
            return await LobbyManager.singleton.UpdatePlayerData(_localLobbyPlayerData.id, _localLobbyPlayerData.Serialize()); 
        }

        internal int GetMapIndex()
        {
            return _lobbyData.mapIndex;
        }

        internal async Task<bool> SetSelectedMap(int currentMapIndex, string sceneName)
        {
            _lobbyData.mapIndex = currentMapIndex;
            _lobbyData.sceneName = sceneName;

            return await LobbyManager.singleton.UpdateLobbyData(_lobbyData.Serialize());
        }


//----------------------------------------------------------------------------------
// Relay Service and starting / joining game
//----------------------------------------------------------------------------------

        internal async Task<bool> StartGame()
        {
            string joinRelayCode;
            try
            {
                joinRelayCode = await RelayManager.singleton.CreateRelay(MAX_PLAYERS);
                _isTransitioning = true;
            }
            catch (Exception)
            {
                Debug.LogError("Failed to start relay server");
                return false;
            }

            _lobbyData.joinRelayCode = joinRelayCode;
            await LobbyManager.singleton.UpdateLobbyData(_lobbyData.Serialize());

            string allocationId = RelayManager.singleton.allocationId.ToString();
            string connectionData = RelayManager.singleton.connectionData.ToString();

            await LobbyManager.singleton.UpdatePlayerData(_localLobbyPlayerData.id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

            await SceneManager.LoadSceneAsync(_lobbyData.sceneName);

            return true;
        }

        private async Task<bool> JoinRelayServer(string joinRelayCode)
        {
            try
            {
                _isTransitioning = true;
                await RelayManager.singleton.JoinRelay(joinRelayCode);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join relay server {e}");
                return false;
            }

            string allocationId = RelayManager.singleton.allocationId.ToString();
            string connectionData = RelayManager.singleton.connectionData.ToString();

            await LobbyManager.singleton.UpdatePlayerData(_localLobbyPlayerData.id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

            return true;
        }

    }
}
