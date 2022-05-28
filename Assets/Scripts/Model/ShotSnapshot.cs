using UnityEngine;

namespace Model
{
    public class ShotSnapshot
    {
        public readonly float Time;
        public readonly int Target;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;

        public ShotSnapshot(int target, Vector3 position, Vector3 direction, float time = 0f)
        {
            Position = position;
            Direction = direction;
            Target = target;
            Time = time;
        }

        public ShotSnapshot Timed(float time)
        {
            return new ShotSnapshot(Target, Position, Direction, time);
        }

        public override string ToString()
        {
            return $"s{Target} {Position.ToString()}";
        }
    }
}