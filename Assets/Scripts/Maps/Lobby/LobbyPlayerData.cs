using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;

public class LobbyPlayerData
{

    private string _id; public string id { get { return _id; } }
    private string _gamertag; public string gamertag { get { return _gamertag; } }
    private bool _isReady; public bool isReady { get { return _isReady; } }

    public void Initialize(string id, string gamertag)
    {
        _id = id;
        _gamertag = gamertag;
    }

    public void Initialize(Dictionary<string, PlayerDataObject> playerData)
    {

    }

    public void UpdateState(Dictionary<string, PlayerDataObject> playerData)
    {
        if (playerData.ContainsKey("Id"))
        {
            _id = playerData["Id"].Value;
        }
        if (playerData.ContainsKey("Gamertag"))
        {
            _gamertag = playerData["Gamertag"].Value;
        }
        if (playerData.ContainsKey("IsReady"))
        {
            _isReady = playerData["Id"].Value == "True";
        }
    }

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>()
        {
            {"Id", _id},
            {"Gamertag", _gamertag},
            {"IsReady", _isReady.ToString()}
        };
    }

}
