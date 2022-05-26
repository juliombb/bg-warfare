using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerSnapshot
    {
        public readonly int Sequence;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlayerSnapshot(int sequence, Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            Sequence = sequence;
        }

        public override string ToString()
        {
            return $"s{Sequence} {Position.ToString()}";
        }
    }
}