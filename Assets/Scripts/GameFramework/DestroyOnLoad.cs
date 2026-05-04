using Unity.Netcode;
using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{

    private void Awake()
    {
        if (Object.FindObjectsByType<NetworkManager>().Length > 1)
        {
            DestroyImmediate(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

}
