using Game;
using System.Collections;
using UnityEngine;
using System;

public class ShootAnimation : MonoBehaviour
{
    [SerializeField] TracerAnimation tracerPrefab;
    [SerializeField] SigilAnimation sigilPrefab;

    [SerializeField] Transform[] origins;

    [SerializeField] private PlayerController playerController;

    private void OnEnable()
    {
        playerController.OnShoot += PlayShootAnimation;
    }

    private void OnDisable()
    {
        playerController.OnShoot -= PlayShootAnimation;
    }

    private void PlayShootAnimation(Vector3 destination)
    {
        Transform origin = RandomizeOrigin();

        TracerAnimation tracer = Instantiate(tracerPrefab);
        tracer.Play(origin, destination);

        SigilAnimation sigil = Instantiate(sigilPrefab);
        sigil.Play(origin);
    }

    private Transform RandomizeOrigin()
    {
        System.Random rnd = new System.Random();

        return origins[rnd.Next(0, origins.Length)];
    }
}
