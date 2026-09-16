using Assets.Scripts.Game.PlayerScripts.CharacterScripts.AbilityScripts;
using Game;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;


namespace Assets.Scripts.Game.PlayerScripts.NetworkObjects
{
    public class ShepheardAbilities : AbilitySet
    {
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
                Throw(aimTransform.position, aimTransform.forward, -20, 40, 0.9f, 0.99f, 2, OnSmokeTriggered);
            }
            else
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

            CreateObject(transform.position + Vector3.down * 2f, 50f, new Vector3(2, 0.3f, 2), null);
        }

        public override void AbilityThree()
        {
            if (!IsOwner)
            {
                return;
            }

            NetworkObject target = TargetRaycast(aimTransform.position, aimTransform.forward, 30f);

            if (target == null)
            {
                Debug.Log("Missed target");
            }


        }

        private void OnSmokeTriggered(Vector3 lastPosition)
        {
            throwingSmoke = false;
            Smoke(lastPosition, 30f, 15f, -10f);
        }
    }

}