using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game.PlayerScripts.NetworkObjects
{
    public class TransformState :
        INetworkSerializable,
        IEquatable<TransformState>

    {
        public int tick;
        public Vector3 position;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();

                reader.ReadValueSafe(out tick);
                reader.ReadValueSafe(out position);
                reader.ReadValueSafe(out velocity);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();

                writer.WriteValueSafe(tick);
                writer.WriteValueSafe(position);
                writer.WriteValueSafe(velocity);
            }
        }

        public bool Equals(TransformState other)
        {
            return tick == other.tick &&
                   position == other.position &&
                   velocity == other.velocity;
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
                velocity);
        }
    }
}
