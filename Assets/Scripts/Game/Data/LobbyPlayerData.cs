using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;

public class LobbyPlayerData
{

    private string _id; public string id { get { return _id; } }
    private string _gamertag; public string gamertag { get { return _gamertag; } }
    private bool _isReady; public bool isReady { get { return _isReady; } set { _isReady = value;  } }
    private int _characterIndex; public int characterIndex { get { return _characterIndex; } set { _characterIndex = value; } }
    private string _characterName; public string characterName { get { return _characterName; } set { _characterName = value; } }


    public void Initialize(string id, string gamertag)
    {
        _id = id;
        _gamertag = gamertag;
    }

    public void Initialize(Dictionary<string, PlayerDataObject> playerData)
    {
        UpdateState(playerData);
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
            bool.TryParse(playerData["IsReady"].Value, out _isReady);
        }
        if (playerData.ContainsKey("CharacterName"))
        {
            _characterName = playerData["CharacterName"].Value;
        }
        if (playerData.ContainsKey("CharacterIndex"))
        {
            int.TryParse(playerData["CharacterIndex"].Value, out _characterIndex);
        }
    }

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>()
        {
            {"Id", _id},
            {"Gamertag", _gamertag},
            {"IsReady", _isReady.ToString()},
            {"CharacterName", _characterName ?? ""},
            {"CharacterIndex", _characterIndex.ToString() }
        };
    }

}
