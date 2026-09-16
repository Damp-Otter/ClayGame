using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;


namespace Assets.Scripts.Game.PlayerScripts.NetworkObjects
{
    public class ShepheardAbilities : AbilitySet
    {
        [SerializeField] private Transform aimTransform;

        private bool throwingSmoke = false;

        public override void AbilityOne()
        {
            if (!IsOwner)
            {
                return;
            }

            Debug.Log("triggered ability 1");

            if (!throwingSmoke)
            {
                throwingSmoke = true;
                Throw(aimTransform.position, aimTransform.forward, -20, 40, 0.9f, 0.99f,2, OnSmokeTriggered);
            } else
            {
                DestroyThrowable();
            }
        }

        public override void AbilityTwo()
        {
            if (!IsOwner)
            {
                return;
            }

            Smoke(transform.position, 10f, 15f, -40f);
        }

        public override void AbilityThree()
        {
            Debug.Log("Ability 3");
        }

        private void OnSmokeTriggered(Vector3 lastPosition)
        {
            throwingSmoke = false;
            Smoke(lastPosition, 30f, 15f, -10f);
        }
    }

}