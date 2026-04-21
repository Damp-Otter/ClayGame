using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Authentication and matchmaking stuff will eventually happen, 
/// </summary>

public class MainMenu : MonoBehaviour
{

    [SerializeField] private Button serverButton = null;
    [SerializeField] private Button clientButton = null;

    private void Start()
    {

        #if !UNITY_EDITOR && UNITY_SERVER
        OnServerClicked();
        return
        #endif


        serverButton.onClick.AddListener(OnServerClicked);
        clientButton.onClick.AddListener(OnClientClicked);

    }


    private void OnServerClicked()
    {
        ConnectionManager.singleton.InitializeAsServer(5678);
        SceneManager.LoadScene(1);

    }


    private void OnClientClicked()
    {

        ConnectionManager.singleton.InitializeAsClient("127.0.0.1", 5678);
        SceneManager.LoadScene(1);

    }

}
