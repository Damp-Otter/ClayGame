using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerTerminal : NetworkBehaviour
{
    private static PlayerTerminal _singleton; public static PlayerTerminal singleton { get { return _singleton; } }
    [SerializeField] private List<GameObject> _characterPrefabs;

    protected override void OnNetworkPostSpawn()
    {
        _singleton = this;
        base.OnNetworkPostSpawn();


        Debug.Log($"Attempting to spawn character, ID: {AuthenticationService.Instance.PlayerId}");
        if (IsClient)
        {
            SpawnPlayerRpc(AuthenticationService.Instance.PlayerId);
        }
        

    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPlayerRpc(string clientId, RpcParams rpcParams = default)
    {
        ulong id = rpcParams.Receive.SenderClientId;

        int characterIndex = LobbyPlayerManager.singleton.GetPlayer(clientId).characterIndex;
        var playerPrefab = _characterPrefabs[characterIndex];

        Transform spawnPoint = SpawnPoints.singleton.GetPointInOrder();
        var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
    }

}
