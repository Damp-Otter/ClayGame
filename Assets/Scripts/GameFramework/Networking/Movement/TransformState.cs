using System;
using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Networking.Movement
{
    public struct TransformState :
        INetworkSerializable,
        IEquatable<TransformState>
    {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public float verticalVelocity;
        public bool hasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();

                reader.ReadValueSafe(out tick);
                reader.ReadValueSafe(out position);
                reader.ReadValueSafe(out rotation);
                reader.ReadValueSafe(out verticalVelocity);
                reader.ReadValueSafe(out hasStartedMoving);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();

                writer.WriteValueSafe(tick);
                writer.WriteValueSafe(position);
                writer.WriteValueSafe(rotation);
                writer.WriteValueSafe(verticalVelocity);
                writer.WriteValueSafe(hasStartedMoving);
            }
        }

        public bool Equals(TransformState other)
        {
            return tick == other.tick &&
                   position == other.position &&
                   rotation == other.rotation &&
                   hasStartedMoving == other.hasStartedMoving;
        }

        public override bool Equals(object obj)
        {
            return obj is TransformState other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                tick,
                position,
                rotation,
                hasStartedMoving);
        }
    }
}