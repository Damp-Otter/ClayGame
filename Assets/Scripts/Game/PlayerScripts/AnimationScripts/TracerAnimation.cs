using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.UI.Image;

public class TracerAnimation : MonoBehaviour
{
    [SerializeField] private LineRenderer _tracer;
    [SerializeField] private float _duration;
    [SerializeField] private float _proportionalRaySize;
    private float _raySize;

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        _tracer.positionCount = 2;
    }

    public void Play(Transform origin, Vector3 destination)
    {
        SetDissolve(0f);

        _tracer.enabled = true;

        StartCoroutine(AnimateTracer(origin.position, destination));
    }

    private IEnumerator AnimateTracer(Vector3 origin, Vector3 destination)
    {
        float elapsed = 0f;
        _raySize = _proportionalRaySize * (origin - destination).magnitude;

        while (elapsed < _duration)
        {
            float t = elapsed / _duration;
            SetDissolve(Mathf.Lerp(0f, 1, t));
            SetPosition(origin, destination, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetDissolve(1f);
        Destroy(gameObject);
    }

    private void SetDissolve(float value)
    {
        _tracer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_Dissolve", value);
        _tracer.SetPropertyBlock(propertyBlock);
    }

    private void SetPosition(Vector3 origin, Vector3 destination, float time)
    {
        Vector3 direction = (destination - origin).normalized;

        Vector3 tail = Vector3.Lerp(origin, destination - _raySize * direction, time);
        Vector3 head = Vector3.Lerp(origin + _raySize * direction, destination, time);

        _tracer.SetPosition(0, tail);
        _tracer.SetPosition(1, head);
    }
}
