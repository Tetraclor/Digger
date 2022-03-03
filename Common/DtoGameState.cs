using GameCore;
using System;

namespace Common
{
    public class DtoGameState
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
    }

    public class DtoCreatureTransformation
    {
        public CreatureCommand Command { get; set; }
        public string CreatureTypeName { get; set; }
        public Point Location { get; set; }
        public Point TargetLogicalLocation { get; set; }

        public DtoCreatureTransformation() { }
        public DtoCreatureTransformation(CreatureTransformation creatureTransformation)
        {
            Command = creatureTransformation.Command;
            CreatureTypeName = creatureTransformation.Creature.GetType().Name;
            Location = creatureTransformation.Location;
            TargetLogicalLocation = creatureTransformation.TargetLogicalLocation;
        }
        public CreatureTransformation Map()
        {
            return new CreatureTransformation()
            {
                Command = Command,
                Location = Location,
                TargetLogicalLocation = TargetLogicalLocation,
                Creature = CreatureMapCreator.CreateCreatureByTypeName(CreatureTypeName)
            };
        }
    }

    public class TypeGetter<T>
    {
        public static Type Type = typeof(T);
    }
}
