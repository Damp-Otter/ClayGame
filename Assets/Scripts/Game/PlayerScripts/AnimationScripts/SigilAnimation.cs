using UnityEngine;

public class SigilAnimation : MonoBehaviour
{
    [SerializeField] ParticleSystem[] sigils;

    public void Play(Transform origin)
    {
        transform.position = origin.position;
        transform.rotation = origin.rotation;

        foreach (ParticleSystem sigil in sigils)
        {
            sigil.Play();

            Destroy(gameObject, sigil.main.duration + sigil.main.startLifetime.constantMax);
        }
    }
}
