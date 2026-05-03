using GameFramework.Data;
using GameFramework.Events;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameLobbyManager : MonoBehaviour
    {
        private static GameLobbyManager _singleton; public static GameLobbyManager singleton { get { return _singleton; } }
        private const int MAX_PLAYERS = 5;

        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();
        private LobbyPlayerData _localLobbyPlayerData;
        private LobbyData _lobbyData;

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

        private void OnLobbyUpdated(Lobby lobby)
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

        internal async Task<bool> SetSelectedMap(int currentMapIndex)
        {
            _lobbyData.mapIndex = currentMapIndex;

            return await LobbyManager.singleton.UpdateLobbyData(_lobbyData.Serialize());
        }


//----------------------------------------------------------------------------------

        internal async Task StartGame(string sceneName)
        {
            Debug.Log($"RelayManager singleton: {RelayManager.singleton}");

            string joinRelayCode = await RelayManager.singleton.CreateRelay(MAX_PLAYERS);

            _lobbyData.SetJoinRelayCode(joinRelayCode);
            await LobbyManager.singleton.UpdateLobbyData(_lobbyData.Serialize());

            string allocationId = RelayManager.singleton.allocationId.ToString();
            string connectionData = RelayManager.singleton.connectionData.ToString();

            await LobbyManager.singleton.UpdatePlayerData(_localLobbyPlayerData.id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

            await SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
