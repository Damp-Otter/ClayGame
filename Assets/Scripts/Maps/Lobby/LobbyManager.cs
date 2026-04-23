using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Collections;

public class LobbyManager : GameLobbyManager
{
    private static LobbyManager _singleton; public static LobbyManager singleton { get { return _singleton; } }

    private Lobby _lobby;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _refreshLobbyCoroutine;
    private float _heartBeatInterval = 6f;
    private float _refreshInterval = 1f;


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


    public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data)
    {

        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);

        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);

        CreateLobbyOptions options = new CreateLobbyOptions()
        {
            IsPrivate = isPrivate,
            Player = player,
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers);
        }
        catch(Exception)
        {
            Debug.LogError("Failed to create lobby");
        }

        Debug.Log($"Lobby created with ID: {_lobby.Id}");

        _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, _heartBeatInterval));
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, _refreshInterval));

        return true;
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

            if(newLobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = newLobby;
            }

            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }


    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
    {

        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

        foreach(var (key, value) in data)
        {
            playerData.Add(key, new PlayerDataObject(
                visibility: PlayerDataObject.VisibilityOptions.Member,
                value: value));
        }

        return playerData;

    }

    public void OnApplicationQuit()
    {
        if(_lobby!=null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
        }
    }

    public override string GetLobbyCode()
    {
        return _lobby?.LobbyCode;
    }

    public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

        options.Player = player;

        bool succeeded;
        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, null);
            succeeded = true;
        }
        catch(Exception)
        {
            succeeded = false;
            Debug.LogError("Failed to join lobby");
        }

        StartCoroutine(RefreshLobbyCoroutine(player.Id, _refreshInterval));

        return succeeded;
    }

    internal List<Dictionary<string, PlayerDataObject>> GetPlayersData()
    {
        List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();

        foreach(var player in _lobby.Players)
        {
            data.Add(player.Data);
        }

        return data;
    }
}
