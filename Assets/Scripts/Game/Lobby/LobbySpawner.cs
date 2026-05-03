using UnityEngine;
using System.Collections.Generic;
using Game.Events;
using NUnit.Framework;

namespace Game
{
	public class LobbySpawner: MonoBehaviour
	{

		[SerializeField] private List<LobbyPlayer> _players;

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLobbyUpdated()
        {
            List<LobbyPlayerData> playerDatas = GameLobbyManager.singleton.GetPlayers();

            for(int i = 0; i < playerDatas.Count; i++)
            {
                LobbyPlayerData data = playerDatas[i];
                _players[i].SetData(data);
            }

        }

    }
}