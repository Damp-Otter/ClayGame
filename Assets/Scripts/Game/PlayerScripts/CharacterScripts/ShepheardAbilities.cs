using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShepheardAbilities : AbilitySet
{
    [SerializeField] private Transform aimTransform;


    public override void AbilityOne()
    {
        Throw(aimTransform.position, aimTransform.forward * 0.8f + aimTransform.up * 0.2f, -40, 40, 0.5f, 2, OnSmokeTriggered);
    }

    public override void AbilityTwo()
    {
        Debug.Log("Ability 2");
    }

    public override void AbilityThree()
    {
        Debug.Log("Ability 3");
    }

    private void OnSmokeTriggered(Vector3 lastPosition)
    {
        Debug.Log($"Throwable destroyed at position {lastPosition.ToString()}");

        Smoke(lastPosition, 10f, 15f);
    }
}
