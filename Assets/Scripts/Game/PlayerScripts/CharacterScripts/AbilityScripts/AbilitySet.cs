using Assets.Scripts.Game.PlayerScripts.NetworkObjects;
using Game;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Game.PlayerScripts.CharacterScripts.AbilityScripts
{
    public abstract class AbilitySet : NetworkBehaviour
    {

        [SerializeField] protected Transform aimTransform;
        [SerializeField] protected GameObject _throwablePrefab;
        private NetworkObject _throwableObject;
        private Action<Vector3> _onThrowableDestroyed;
        protected NetworkObjectReference _throwableReference;



        [SerializeField] protected GameObject _smokePrefab;
        private Action<Vector3> _onSmokeDestroyed;



        [SerializeField] protected GameObject _objectPrefab;
        private Action<Vector3> _onObjectDestroyed;



        [SerializeField] protected LayerMask _targetLayerMask;
        private NetworkObject _lastTargeted;



        [SerializeField] protected GameObject _homingPrefab;
        private NetworkObject[] _homingObjects;
        private Action<Vector3> _onHomingDestroyed;
        protected NetworkObjectReference _homingReference;



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
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ThrowServerRpc(Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float resistance, float lifespan, RpcParams rpcParams = default)
        {
            var instance = Instantiate(_throwablePrefab);

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            NetworkThrowableComponent throwable = instance.GetComponent<NetworkThrowableComponent>();

            throwable.OnThrowableDestroyed += (pos) =>
            {
                ThrowableDestroyedClientRpc(pos);
            };

            instance.transform.position = position + direction * 1;
            instance.transform.forward = direction;
            instance.GetComponent<NetworkObject>().Spawn();

            SetThrowableClientRpc(new NetworkObjectReference(networkObject), position, direction, gravity, velocity, elasticity, resistance, lifespan);
        }


        [ClientRpc]
        private void SetThrowableClientRpc(NetworkObjectReference reference, Vector3 position, Vector3 direction, float gravity, float velocity, float elasticity, float resistance, float lifespan)
        {

            if (reference.TryGet(out NetworkObject networkObject))
            {
                _throwableObject = networkObject;
            }

            NetworkThrowableComponent throwable = _throwableObject.GetComponent<NetworkThrowableComponent>();

            throwable.lifespan = lifespan;
            throwable.velocity = velocity * direction;
            throwable.gravity = gravity;
            throwable.elasticity = elasticity;
            throwable.resistance = resistance;
            throwable.startSimulation = true;
        }


        [ClientRpc]
        private void ThrowableDestroyedClientRpc(Vector3 position)
        {
            if (IsServer)
            {
                return;
            }

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


        // ----------------------------------------------------------------
        // Objects
        // ----------------------------------------------------------------


        protected void CreateObject(Vector3 position, float lifespan, Vector3 scale, Action<Vector3> onObjectDestroyed)
        {
            _onObjectDestroyed = onObjectDestroyed;

            CreateObjectServerRpc(position, lifespan, scale);
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void CreateObjectServerRpc(Vector3 position, float lifespan, Vector3 scale, RpcParams rpcParams = default)
        {
            var instance = Instantiate(_objectPrefab);

            NetworkObjectComponent newObject = instance.GetComponent<NetworkObjectComponent>();

            newObject.OnObjectDestroyed += (pos) =>
            {
                _onObjectDestroyed?.Invoke(pos);
            };

            newObject.lifespan.Value = lifespan;
            newObject.scale.Value = scale;

            if (newObject.damageController == null)
            {
                Debug.Log("I fucking knew it");
            }

            instance.transform.position = position;

            instance.GetComponent<NetworkObject>().Spawn();
        }


        // ----------------------------------------------------------------
        // TargetRaycast
        // ----------------------------------------------------------------

        protected NetworkObject TargetRaycast(Vector3 position, Vector3 direction, float range)
        {
            TargetRaycastServerRpc(position, direction, range);

            return _lastTargeted;
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void TargetRaycastServerRpc(Vector3 origin, Vector3 direction, float range, RpcParams rpcParams = default)
        {
            Debug.Log($"LayerMask {_targetLayerMask}");

            Debug.DrawLine(origin, origin + direction * range, Color.red, 50f);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, _targetLayerMask))
            {
                Debug.Log("Hit");

                SetTargetClientRpc(rpcParams.Receive.SenderClientId, new NetworkObjectReference(hit.collider.gameObject));
            }
            else
            {
                Debug.Log("Miss");

                SetTargetClientRpc(rpcParams.Receive.SenderClientId);
            }
        }



        [ClientRpc]
        private void SetTargetClientRpc(ulong targetClientId, [Optional] NetworkObjectReference reference)
        {
            if (NetworkManager.Singleton.LocalClientId != targetClientId)
            {
                return;
            }

            if (reference.TryGet(out NetworkObject networkObject))
            {
                _lastTargeted = networkObject;
            }
            else
            {
                _lastTargeted = null;
            }
        }


        // ----------------------------------------------------------------
        // Homing
        // ----------------------------------------------------------------

        protected void Homing(Vector3 position, Vector3 direction, int projectiles, float velocity, float elasticity, float resistance, float lifespan, Action<Vector3> onHomingDestroyed)
        {
            _onHomingDestroyed = onHomingDestroyed;

            HomingServerRpc(position, direction, velocity, elasticity, resistance, lifespan);
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void HomingServerRpc(Vector3 position, Vector3 direction, int projectiles, float velocity, float elasticity, float resistance, float lifespan, RpcParams rpcParams = default)
        {
            NetworkObjectReference[] references = new NetworkObjectReference[projectiles];

            for (int i = 0; i < projectiles; i++)
            {
                var instance = Instantiate(_homingPrefab);

                NetworkObject networkObject = instance.GetComponent<NetworkObject>();
                NetworkHomingComponent homing = instance.GetComponent<NetworkHomingComponent>();

                homing.OnHomingDestroyed += (pos) =>
                {
                    HomingDestroyedClientRpc(pos);
                };

                instance.transform.position = position + direction * 1;
                instance.transform.forward = direction;
                instance.GetComponent<NetworkObject>().Spawn();

                references[i] = new NetworkObjectReference(networkObject);
            }

            SetHomingClientRpc(references, position, direction, velocity, elasticity, resistance, lifespan);
        }


        [ClientRpc]
        private void SetHomingClientRpc(NetworkObjectReference[] references, Vector3 position, Vector3 direction, float velocity, float elasticity, float resistance, float lifespan)
        {
            for (int i = 0; i < references.Length; i++)
            {
                if (references[i].TryGet(out NetworkObject networkObject))
                {
                    _homingObjects[i] = networkObject;
                }

                NetworkThrowableComponent homing = _homingObjects[i].GetComponent<NetworkThrowableComponent>();

                homing.lifespan = lifespan;
                homing.velocity = velocity * direction;
                homing.elasticity = elasticity;
                homing.resistance = resistance;
                homing.startSimulation = true;
            }
        }


        [ClientRpc]
        private void HomingDestroyedClientRpc(Vector3 position)
        {
            if (IsServer)
            {
                return;
            }

            _onThrowableDestroyed?.Invoke(position);

            bool homingObjectsEmpty = true;
            foreach (NetworkObject homing in _homingObjects)
            {
                if (homing != null)
                {
                    homingObjectsEmpty = true;
                }
            }

            if (homingObjectsEmpty)
            {
                Debug.Log("All destroyed");
            }
        }


        protected void DestroyHoming()
        {
            DestroyHomingServerRpc(new NetworkObjectReference(_throwableObject));
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DestroyHomingServerRpc(NetworkObjectReference reference)
        {
            if (reference.TryGet(out NetworkObject networkObject))
            {
                NetworkThrowableComponent throwable = networkObject.GetComponent<NetworkThrowableComponent>();

                throwable.DestroyHoming();
            }
        }
    }

}
