using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Game.PlayerScripts.NetworkObjects
{
    public abstract class AbilitySet : NetworkBehaviour
    {

        [SerializeField] private GameObject _throwablePrefab;
        private NetworkObject _throwableObject;
        private Action<Vector3> _onThrowableDestroyed;
        protected NetworkObjectReference _throwableReference;

        [SerializeField] private GameObject _smokePrefab;
        private Action<Vector3> _onSmokeDestroyed;

        public abstract void AbilityOne();

        public abstract void AbilityTwo();

        public abstract void AbilityThree();


        // ----------------------------------------------------------------
        // Throwables
        // ----------------------------------------------------------------


        protected void Throw(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float resistance, float lifespan, Action<Vector3> onThrowableDestroyed)
        {
            _onThrowableDestroyed = onThrowableDestroyed;

            ThrowServerRpc(position, direction, gravity, velocity, elasticity, resistance, lifespan);

            NetworkThrowableComponent throwable = _throwableObject.GetComponent<NetworkThrowableComponent>();

            throwable.lifespan = lifespan;
            throwable.velocity = velocity * direction;
            throwable.gravity = gravity;
            throwable.elasticity = elasticity;
            throwable.resistance = resistance;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ThrowServerRpc(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float resistance, float lifespan, RpcParams rpcParams = default)
        {
            var instance = Instantiate(_throwablePrefab);

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            NetworkThrowableComponent throwable = instance.GetComponent<NetworkThrowableComponent>();

            throwable.OnThrowableDestroyed += (pos) =>
            {
                ThrowableDestroyedClientRpc(pos, rpcParams.Receive.SenderClientId);
            };

            instance.transform.position = position + direction * 1;
            instance.transform.forward = direction;
            instance.GetComponent<NetworkObject>().Spawn();

            SetThrowableClientRpc(new NetworkObjectReference(networkObject));
        }

        [ClientRpc]
        private void SetThrowableClientRpc(NetworkObjectReference reference)
        {
            Debug.Log($"Setting on the {(IsServer ? "Server" : "Client")}");

            if (reference.TryGet(out NetworkObject networkObject))
            {
                _throwableObject = networkObject;
            }
            else
            {
                Debug.LogWarning("Failed to get Network object from reference :(");
            }
        }

        [ClientRpc]
        private void ThrowableDestroyedClientRpc(Vector3 position, ulong targetClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            _onThrowableDestroyed?.Invoke(position);
        }

        protected void DestroyThrowable()
        {
            DestroyThrowableServerRpc(new NetworkObjectReference(_throwableObject));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DestroyThrowableServerRpc(NetworkObjectReference reference)
        {
            if (reference.TryGet(out NetworkObject networkObject))
            {
                NetworkThrowableComponent throwable = networkObject.GetComponent<NetworkThrowableComponent>();

                throwable.DestroyThrowable();
            }
        }


        // ----------------------------------------------------------------
        // Smokes
        // ----------------------------------------------------------------


        protected void Smoke(Vector3 position, float lifespan, float scale, float gravity)
        {
            SmokeServerRpc(position, lifespan, scale, gravity);
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SmokeServerRpc(Vector3 position, float lifespan, float scale, float gravity, RpcParams rpcParams = default)
        {
            var instance = Instantiate(_smokePrefab);

            NetworkSmokeComponent smoke = instance.GetComponent<NetworkSmokeComponent>();

            smoke.OnSmokeDestroyed += (pos) =>
            {
                _onSmokeDestroyed?.Invoke(pos);
            };

            smoke.gravity.Value = gravity;
            smoke.lifespan.Value = lifespan;
            smoke.scale.Value = scale;

            instance.transform.position = position;

            instance.GetComponent<NetworkObject>().Spawn();
        }
    }

}
