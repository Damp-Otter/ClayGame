#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class RendererFeatureReset
{
    static RendererFeatureReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        var renderers = Resources.FindObjectsOfTypeAll<UniversalRendererData>();

        foreach (var renderer in renderers)
        {
            foreach (var feature in renderer.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fullscreen)
                {
                    fullscreen.SetActive(false);
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif