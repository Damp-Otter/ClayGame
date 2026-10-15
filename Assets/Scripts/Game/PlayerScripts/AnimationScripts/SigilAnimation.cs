using UnityEngine;

public class SigilAnimation : MonoBehaviour
{
    [SerializeField] ParticleSystem sigil;

    public void Play(Transform origin)
    {
        Debug.Log($"Sigil played on client {Unity.Netcode.NetworkManager.Singleton.LocalClientId}");

        transform.position = origin.position;
        transform.rotation = origin.rotation;

        sigil.Play();

        Debug.Log(
            $"Position: {sigil.transform.position}, " +
            $"Particles: {sigil.particleCount}, " +
            $"Renderer: {sigil.GetComponent<ParticleSystemRenderer>().enabled}, " +
            $"Layer: {LayerMask.LayerToName(sigil.gameObject.layer)}"
        );

        Destroy(gameObject, sigil.main.duration + sigil.main.startLifetime.constantMax);
    }

}
