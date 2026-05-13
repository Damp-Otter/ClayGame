using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerManager : MonoBehaviour
{
    public static LobbyPlayerManager singleton;

    public Dictionary<string, LobbyPlayerData> players = new Dictionary<string, LobbyPlayerData>();
    public Dictionary<ulong, string> clientToAuth = new Dictionary<ulong, string>();


    internal LobbyPlayerData GetPlayer(string playerId)
    {
        foreach(var (id, data) in players)
        {
            if (id == playerId)
            {
                return data;
            }
        }
        Debug.LogWarning("Failed to find playerID");
        return null;
    }

    private void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;

        DontDestroyOnLoad(gameObject);
    }
}

