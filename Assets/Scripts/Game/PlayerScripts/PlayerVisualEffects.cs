using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PlayerVisualEffects : NetworkBehaviour
{

    [SerializeField] private FullScreenPassRendererFeature smokeFogFeature;

    private List<Collider> currentColliders = new List<Collider>();


#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            smokeFogFeature.SetActive(false);
        }
    }
#endif


    public void HandleSmokeTriggered(bool enter, Collider collider)
    {
        if (!IsOwner)
        {
            return;
        }

        smokeFogFeature.passMaterial.SetFloat("_Distance", 3f);
        smokeFogFeature.passMaterial.SetFloat("_Strength", 1f);

        if (enter)
        {
            currentColliders.Add(collider);
            smokeFogFeature.SetActive(enter);
            Debug.Log("Enter Smoke");
        }
        else
        {
            currentColliders.Remove(collider);
            Debug.Log("Exit Smoke");
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Debug.Log($"Collider count: {currentColliders.Count}");

        currentColliders.RemoveAll(collider => collider == null);

        if (currentColliders.Count == 0)
        {
            smokeFogFeature.SetActive(false);
            return;
        }

        foreach (Collider collider in currentColliders)
        {
            float absDistanceFromSmokeEdge = Mathf.Abs((collider.transform.position - transform.position).magnitude - (collider.transform.localScale.x / 2));
            float distanceFromSmokeEdge = (collider.transform.position - transform.position).magnitude - (collider.transform.localScale.x / 2);
            if (absDistanceFromSmokeEdge < 0.3f)
            {
                smokeFogFeature.passMaterial.SetFloat("_Distance", 5 * absDistanceFromSmokeEdge);
                return;
            }
            else
            {
                smokeFogFeature.passMaterial.SetFloat("_Distance", 3);
            }
        }
    }

}