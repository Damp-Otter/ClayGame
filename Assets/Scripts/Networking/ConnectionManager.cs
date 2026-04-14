using UnityEngine;

public class ConnectionManager : MonoBehaviour
{

    private static ConnectionManager _singleton; public static ConnectionManager singleton { get { return _singleton; } }
    private string _connectionIP; public string connectionIP { get { return _connectionIP; } }
    private ushort _connectionPort; public ushort connectionPort { get { return _connectionPort; } }
    private Type _connectionType; public Type connectionType { get { return _connectionType; } }


    public enum Type
    {
        None = 0, Client = 1, Server = 2
    }


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


    public void InitializeAsServer(ushort port)
    {
        _connectionPort = port;
        _connectionType = Type.Server;
    }


    public void InitializeAsClient(string ip, ushort port)
    {
        _connectionIP = ip;
        _connectionPort = port;
        _connectionType = Type.Client;
    }


}
