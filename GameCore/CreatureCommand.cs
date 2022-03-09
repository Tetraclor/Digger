using System;

namespace GameCore
{
    public class CreatureCommand
    {
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
        public ICreature TransformTo { get; set; }

        public Point MoveFrom(int x, int y) => new Point(x + DeltaX, y + DeltaY);
        public Point MoveFrom(Point point) => new Point(point.X + DeltaX, point.Y + DeltaY);
        public CreatureCommand Clone() => (CreatureCommand)MemberwiseClone();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (CreatureCommand)obj;
            return DeltaX.Equals(other.DeltaX) &&
                   DeltaY.Equals(other.DeltaY);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(DeltaX, DeltaY, TransformTo).GetHashCode();
        }

        public static bool operator ==(CreatureCommand a, CreatureCommand b) => a.Equals(b);
        public static bool operator !=(CreatureCommand a, CreatureCommand b) => !a.Equals(b);
    }
}
