using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class GameState : IReadOnlyGameState
    {
        public List<IPlayer> Players = new List<IPlayer>();
        public Dictionary<IPlayer, IPlayerCommand> PlayersCommands = new Dictionary<IPlayer, IPlayerCommand>();
        public Dictionary<ICreature, IPlayer> PlayersCreatures = new Dictionary<ICreature, IPlayer>();
        public Dictionary<ICreature, Point> CreaturesLocations = new Dictionary<ICreature, Point>();
        public readonly ICreature[,] Map;
        public int Scores { get; set; }
        public bool IsOver { get; set; }

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        public GameState()
        {
        }

        public GameState(ICreature[,] map)
        {
            Map = map;
            MapWidth = Map.GetLength(0);
            MapHeight = Map.GetLength(1);
        }

        public void SetCreature(Point point, ICreature creature)
        {
            if(creature != null)
                CreaturesLocations[creature] = point;
            Map[point.X, point.Y] = creature;
        }

        public ICreature GetCreatureOrNull(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= MapWidth || point.Y >= MapHeight)
                return null;
            return Map[point.X, point.Y];
        }

        public void AddPlayer(IPlayer player, params ICreature[] bindCreatures)
        {
            foreach (var creature in bindCreatures)
            {
                PlayersCreatures[creature] = player;
            }
            Players.Add(player);
        }

        public IPlayerCommand GetPlayerCommandOrNull(ICreature creature)
        {
            if (PlayersCreatures.TryGetValue(creature, out IPlayer player) == false)
                return null;
            if (PlayersCommands.TryGetValue(player, out IPlayerCommand command) == false)
                return null;
            return command;
        }
    }

    public interface IPlayer
    {
        IPlayerCommand GetCommand(IReadOnlyGameState gameState);
    }

    public interface IPlayerCommand
    {
    }

    public interface IReadOnlyGameState
    {
        ICreature GetCreatureOrNull(Point point);
    }
}
