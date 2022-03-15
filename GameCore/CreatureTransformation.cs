namespace GameCore
{
    public class CreatureTransformation
    {
        public CreatureCommand Command { get; set; }
        public ICreature Creature { get; set; }
        public Point Location { get; set; }
        public Point TargetLogicalLocation { get; set; }

        public CreatureTransformation()
        {

        }

        public CreatureTransformation(Point point, ICreature creature)
        {
            Creature = creature;
            Location = point;
            TargetLogicalLocation = point;
            Command = new CreatureCommand();
        }

        public CreatureTransformation(ICreature creature, Point from, Point to)
        {
            Creature = creature;
            Location = from;
            TargetLogicalLocation = to;
            Command = new CreatureCommand();
        }
    }
}
