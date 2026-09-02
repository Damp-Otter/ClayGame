using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShepheardAbilities : AbilitySet
{
    [SerializeField] private Transform aimTransform;

    private bool throwingSmoke = false;

    public override void AbilityOne()
    {
        if (!throwingSmoke)
        {
            throwingSmoke = true;
            Throw(aimTransform.position, aimTransform.forward * 0.8f + aimTransform.up * 0.2f, -40, 50, 0.6f, 5, OnSmokeTriggered);
        }
        else
        {
            DestroyThrowable();
        }
       
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
        throwingSmoke = false;

        Smoke(lastPosition, 10f, 15f);
    }
}
