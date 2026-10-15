using System.Collections;
using UnityEngine;

public class TracerAnimation : MonoBehaviour
{
    [SerializeField] LineRenderer tracer;
    [SerializeField] private float duration = 0.2f;

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        tracer.positionCount = 2;
    }

    public void Play(Transform origin, Vector3 destination)
    {
        SetDissolve(0f);
        tracer.SetPosition(0, origin.position);
        tracer.SetPosition(1, destination);
        tracer.enabled = true;

        StartCoroutine(AnimateTracer());
    }

    private IEnumerator AnimateTracer()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            SetDissolve(Mathf.Lerp(0f, 1, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetDissolve(1f);
        Destroy(gameObject);
    }

    private void SetDissolve(float value)
    {
        tracer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_Dissolve", value);
        tracer.SetPropertyBlock(propertyBlock);
    }

}
