using System.Transactions;
using UnityEngine;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using System;

#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Game
{
    public class Init : MonoBehaviour
    {

        public static void LoadSceneOnTop(string scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        }

        public static void UnLoadSceneOnTop(string scene)
        {
            int n = SceneManager.sceneCount;
            if (n > 1)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        async void Start()
        {
            LoadSceneOnTop("Loading");

            await UnityServices.InitializeAsync();

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AuthenticationService.Instance.SignedIn += OnSignedIn;

                #if UNITY_EDITOR
                if (ClonesManager.IsClone())
                {
                    string profileId = "Clone_" + Guid.NewGuid().ToString("N");
                    profileId = profileId.Substring(0,10);
                    AuthenticationService.Instance.SwitchProfile(profileId);
                }
                else
                {
                    AuthenticationService.Instance.SwitchProfile("Main");
                }
                #endif

                await AuthenticationService.Instance.SignInAnonymouslyAsync();


                if (AuthenticationService.Instance.IsSignedIn)
                {

                    string username = PlayerPrefs.GetString("Username");
                    if (username == string.Empty)
                    {
                        username = "Player";
                        PlayerPrefs.SetString("Username", username);
                    }

                    UnLoadSceneOnTop("Loading");

                    await SceneManager.LoadSceneAsync("MainMenu");

                }
            }
        }

        private void OnSignedIn()
        {
            Debug.Log($"Signed in, PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
    }
}
