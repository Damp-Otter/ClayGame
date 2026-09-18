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

            CreateObject(transform.position + Vector3.down * 1.7f, 50f, new Vector3(2, 0.3f, 2), null);
        }

        public override void AbilityThree()
        {
            if (!IsOwner)
            {
                return;
            }

            TargetRaycast(aimTransform.position, aimTransform.forward, 500f);
        }

        protected override void OnTargetSet(NetworkObject target)
        {
            if (target != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = (2 * Mathf.PI / 5 * i);
                    float step = 2f;
                    Vector3 startOffset = new Vector3(Mathf.Cos(angle) * step, Mathf.Sin(angle) * step, 0);

                    Vector3 position = aimTransform.position + startOffset;

                    Homing(position, aimTransform.forward, target, 0.4f, 20, 0.1f, i * 45, 1, 1, 6, 10, OnHomingTriggered);
                }
            }
        }

        private void OnHomingTriggered(NetworkObjectReference reference)
        {
            Debug.Log("Triggered");

            if (!reference.TryGet(out NetworkObject networkObject))
            {
                Debug.LogError("Didn't hit a network object, you hit something else or the reference couldnt find one.");
                return;
            }

            Heal(reference, 10, true);
        }

        private void OnSmokeTriggered(Vector3 lastPosition)
        {
            throwingSmoke = false;
            Smoke(lastPosition, 30f, 15f, -10f);
        }
    }

}