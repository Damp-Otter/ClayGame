using Unity.Netcode;
using UnityEngine;

public class PlayerTerminal : NetworkBehaviour
{
    private static PlayerTerminal _singleton; public static PlayerTerminal singleton { get { return _singleton; } }


    protected override void OnNetworkPostSpawn()
    {
        _singleton = this;
        base.OnNetworkPostSpawn();

        if (IsClient)
        {
            SpawnPlayerRpc();
        }

    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPlayerRpc(RpcParams rpcParams = default)
    {
        ulong id = rpcParams.Receive.SenderClientId;
        var playerPrefab = NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList[0].Prefab;
        Transform spawnPoint = SpawnPoints.singleton.GetPointInOrder();
        var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);    
        player.GetComponent<NetworkObject>().SpawnWithOwnership(id, true);
    }

}
