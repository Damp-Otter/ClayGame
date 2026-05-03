using Game;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    private static RelayManager _singleton; public static RelayManager singleton { get { return _singleton; } }

    private string _joinCode; 
    private string _ip;
    private int _port; 
    private byte[] _connectionData; public byte[] connectionData { get { return _connectionData; } }
    private Guid _allocationId; public Guid allocationId { get { return _allocationId; } }


    private void Awake()
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


    public async Task<string> CreateRelay(int maxConnections)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");
        
        _ip = dtlsEndpoint.Host;
        _port = dtlsEndpoint.Port;

        _allocationId = allocation.AllocationId;
        _connectionData = allocation.ConnectionData;


        return _joinCode;
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        _joinCode = joinCode;
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");

        _ip = dtlsEndpoint.Host;
        _port = dtlsEndpoint.Port;

        _allocationId = allocation.AllocationId;
        _connectionData = allocation.ConnectionData;

        return true;
    }

}
