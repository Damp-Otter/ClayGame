using Game;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private void Start()
    {

        // Because the game manager is in the actual game scenes, this runs when the players join

        // Guard in case they join twice
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            return;

        Debug.Log($"GameManager Start running: isHost = {RelayManager.singleton.isHost}");

        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

        if (RelayManager.singleton.isHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    RelayManager.singleton.ip,
                    (ushort)RelayManager.singleton.port,
                    RelayManager.singleton.allocationByteId,
                    RelayManager.singleton.key,
                    RelayManager.singleton.connectionData,
                    true
                );

            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    RelayManager.singleton.ip,
                    (ushort)RelayManager.singleton.port,
                    RelayManager.singleton.allocationByteId,
                    RelayManager.singleton.key,
                    RelayManager.singleton.connectionData,
                    RelayManager.singleton.hostConnectionData,
                    true
                );

            NetworkManager.Singleton.StartClient();
        }
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;
    }
}
