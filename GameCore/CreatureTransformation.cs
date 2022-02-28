namespace GameCore
{
    public class CreatureTransformation
    {
        public CreatureCommand Command { get; set; }
        public ICreature Creature { get; set; }
        public Point Location { get; set; }
        public Point DrawLocation { get; set; }
        public Point TargetLogicalLocation { get; set; }
    }
}
