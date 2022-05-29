using UnityEngine;

namespace Model
{
    public class PlayerSnapshot
    {
        public readonly float Time;
        public readonly int Sequence;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlayerSnapshot(int sequence, Vector3 position, Quaternion rotation, float time = 0f)
        {
            Position = position;
            Rotation = rotation;
            Sequence = sequence;
            Time = time;
        }

        public PlayerSnapshot Timed(float time)
        {
            return new PlayerSnapshot(Sequence, Position, Rotation, time);
        }

        public override string ToString()
        {
            return $"t{Time} s{Sequence} {Position.ToString()}";
        }
    }
}