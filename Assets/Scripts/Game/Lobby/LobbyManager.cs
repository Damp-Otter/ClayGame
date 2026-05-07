using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Collections;
using GameFramework.Events;
using Game;
using Unity.VisualScripting;


namespace Game
{
    public class LobbyManager : MonoBehaviour
    {
        private static LobbyManager _singleton; public static LobbyManager singleton { get { return _singleton; } }

        private Lobby _lobby;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _refreshLobbyCoroutine;
        private float _heartBeatInterval = 6f;
        private float _refreshInterval = 1f;
        private


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


        public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> playerData, Dictionary<string, string> lobbyData)
        {
            Dictionary<string, PlayerDataObject> serializedPlayerData = SerializePlayerData(playerData);
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, serializedPlayerData);

            Dictionary<string, DataObject> serializedLobbyData = SerializeLobbyData(lobbyData);

            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = serializedLobbyData,
                IsPrivate = isPrivate,
                Player = player,
            };

            try
            {
                _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
            }
            catch (Exception)
            {
                Debug.LogError("Failed to create lobby");
            }

            Debug.Log($"Lobby created with ID: {_lobby.Id}");

            LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);

            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, _heartBeatInterval));
            _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, _refreshInterval));

            return true;
        }


        public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

            options.Player = player;

            bool succeeded;
            try
            {
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
                succeeded = true;
            }
            catch (Exception)
            {
                succeeded = false;
                Debug.LogError("Failed to join lobby");
            }

            LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);

            StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, _refreshInterval));

            return succeeded;
        }


        private IEnumerator HeartbeatLobbyCoroutine(string id, float waitTimeSeconds)
        {
            while (true)
            {
                Debug.Log("Heartbeat");
                LobbyService.Instance.SendHeartbeatPingAsync(id);
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }


        private IEnumerator RefreshLobbyCoroutine(string id, float waitTimeSeconds)
        {
            while (true)
            {
                Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(id);
                yield return new WaitUntil(() => task.IsCompleted);
                Lobby newLobby = task.Result;

                if (newLobby.LastUpdated > _lobby.LastUpdated)
                {
                    _lobby = newLobby;
                    LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);
                }

                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }


        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {

            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

            foreach (var (key, value) in data)
            {
                playerData.Add(key, new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Member,
                    value: value));
            }

            return playerData;

        }


        private Dictionary<string, DataObject> SerializeLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>();
            foreach(var (key, value) in data)
            {
                lobbyData.Add(key, new DataObject(
                    visibility: DataObject.VisibilityOptions.Member, // This means only visible to members
                    value: value));
            }

            return lobbyData;
        }


        public void OnApplicationQuit()
        {
            if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            }
        }


        public string GetLobbyCode()
        {
            return _lobby?.LobbyCode;
        }


        internal string GetHostId()
        {
            return _lobby.HostId;
        }


        internal List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();

            foreach (var player in _lobby.Players)
            {
                data.Add(player.Data);
            }

            return data;
        }


        internal async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data, string allocationId = default, string connectionData = default)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);

            UpdatePlayerOptions options = new UpdatePlayerOptions()
            {
                Data = playerData,
                AllocationId = allocationId,
                ConnectionInfo = connectionData
            };
            try
            {
                _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, options);
            }
            catch (Exception)
            {
                Debug.LogError("Failed to update player");
                return false;
            }

            return true;
        }
    

        public async Task<bool> UpdateLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = SerializeLobbyData(data);

            UpdateLobbyOptions options = new UpdateLobbyOptions()
            {
                Data = lobbyData,
            };


            try
            {
                _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, options);
            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }

        public void StopLobbyUpdates()
        {
            if (_refreshLobbyCoroutine != null)
            {
                StopCoroutine(_refreshLobbyCoroutine);
                _refreshLobbyCoroutine = null;
            }
        }

    }

}
