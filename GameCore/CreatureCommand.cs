namespace GameCore
{
    public class CreatureCommand
    {
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
        public ICreature TransformTo { get; set; }
    }
}
