using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using Unity.VisualScripting;

public class GameLobbyManager : MonoBehaviour
{
    private static GameLobbyManager _singleton; public static GameLobbyManager singleton { get { return _singleton; } }
    private const int MAX_PLAYERS = 5;
    private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();
    private LobbyPlayerData _localLobbyPlayerData;


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
        LobbyPlayerData playerData = new LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

        bool succeeded = await LobbyManager.singleton.CreateLobby(MAX_PLAYERS, true, playerData.Serialize());

        return succeeded;
    }


    public virtual string GetLobbyCode()
    {
        return LobbyManager.singleton.GetLobbyCode();
    }


    public async Task<bool> JoinLobby(string code)
    {
        LobbyPlayerData playerData = new LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

        bool succeeded = await LobbyManager.singleton.JoinLobby(code, playerData.Serialize());

        return succeeded;
    }

    private void OnLobbyUpdated(Lobby lobby)
    {
        List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.singleton.GetPlayersData();
        _lobbyPlayerDatas.Clear();

        foreach(Dictionary<string, PlayerDataObject> data in playerData)
        {
            LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
            lobbyPlayerData.Initialize(data);

            if(lobbyPlayerData.id == AuthenticationService.Instance.PlayerId)
            {
                _localLobbyPlayerData = lobbyPlayerData;
            }

            _lobbyPlayerDatas.Add(lobbyPlayerData);
        }
    
    }
}
