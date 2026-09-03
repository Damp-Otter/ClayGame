using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerVisualEffects : MonoBehaviour
{

    [SerializeField] private FullScreenPassRendererFeature smokeFogFeature;

    public void HandleSmoked(bool enter)
    {
        smokeFogFeature.SetActive(enter);
    }

}
