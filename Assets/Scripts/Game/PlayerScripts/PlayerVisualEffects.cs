using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerVisualEffects : MonoBehaviour
{

    [SerializeField] private FullScreenPassRendererFeature smokeFogFeature;

    private List<Collider> currentColliders = new List<Collider>();

    public void HandleSmokeTriggered(bool enter, Collider collider)
    {
        smokeFogFeature.SetActive(enter);
        if (enter)
        {
            currentColliders.Add(collider);
        }
        else
        {
            currentColliders.Remove(collider);
        }
    }

    private void Update()
    {
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
                Debug.Log(1 - (1 - distanceFromSmokeEdge * 2));
                smokeFogFeature.passMaterial.SetFloat("_Distance", absDistanceFromSmokeEdge * 2);
                return;
            }
            else
            {
                smokeFogFeature.passMaterial.SetFloat("_Distance", absDistanceFromSmokeEdge * 2);
            }
        }
    }

}
