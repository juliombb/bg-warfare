using UnityEngine;

namespace Model
{
    public class ShotSnapshot
    {
        public readonly int Target;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;

        public ShotSnapshot(int target, Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
            Target = target;
        }

        public ShotSnapshot WithTarget(int target)
        {
            return new ShotSnapshot(target, Position, Direction);
        }

        public override string ToString()
        {
            return $"s{Target} {Position.ToString()}";
        }
    }
}