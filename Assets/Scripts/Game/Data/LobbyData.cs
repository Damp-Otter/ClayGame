using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using System;
using System.Threading.Tasks;
using Game;

namespace GameFramework.Data
{
	public class LobbyData
	{
		private int _mapIndex; public int mapIndex { get { return _mapIndex; } set { _mapIndex = value; } }
        private string _joinRelayCode; public string joinRelayCode { get { return _joinRelayCode; } set { _joinRelayCode = value; } }
        private string _sceneName; public string sceneName { get { return _sceneName; } set { _sceneName = value; } }


        public void Initialize(int mapIndex) 
		{
			_mapIndex = mapIndex;
		}

		public void Initialize(Dictionary<string, DataObject> lobbyData)
		{
			UpdateState(lobbyData);
		}

		public void UpdateState(Dictionary<string, DataObject> lobbyData)
		{
            if (lobbyData.ContainsKey("mapIndex"))
            {
                _mapIndex = Int32.Parse(lobbyData["mapIndex"].Value);
            }

            if (lobbyData.ContainsKey("joinRelayCode"))
            {
				_joinRelayCode = lobbyData["joinRelayCode"].Value;
            }

            if (lobbyData.ContainsKey("sceneName"))
            {
                _sceneName = lobbyData["sceneName"].Value;
            }

        }

		public Dictionary<string, string> Serialize()
		{
			return new Dictionary<string, string>
			{
				{"mapIndex", mapIndex.ToString()},
				{"joinRelayCode", _joinRelayCode},
				{"sceneName", _sceneName }
			};
		}
    }
}