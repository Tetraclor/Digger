using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class Game
    {

        public List<CreatureTransformation> Animations = new List<CreatureTransformation>();

        public GameState GameState { get; }

        public Game(GameState gameState)
        {
            GameState = gameState;
        }

        public void BeginAct()
        {
            Animations.Clear();
            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                {
                    var creature = GameState.Map[x, y];
                    if (creature == null) continue;
                    var command = creature.Act(GameState, x, y);

                    if (x + command.DeltaX < 0 || x + command.DeltaX >= GameState.MapWidth || y + command.DeltaY < 0 ||
                        y + command.DeltaY >= GameState.MapHeight)
                        throw new Exception($"The object {creature.GetType()} falls out of the game field");

                    Animations.Add(
                        new CreatureTransformation
                        {
                            Command = command,
                            Creature = creature,
                            Location = new Point(x, y),
                            TargetLogicalLocation = new Point(x + command.DeltaX, y + command.DeltaY)
                        });
                }

            Animations = Animations.OrderByDescending(z => z.Creature.TransformPriority()).ToList();
        }

        public void EndAct()
        {
            var creaturesPerLocation = GetCandidatesPerLocation();
            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                    GameState.Map[x, y] = SelectWinnerCandidatePerLocation(creaturesPerLocation, x, y);
        }

        private static ICreature SelectWinnerCandidatePerLocation(List<ICreature>[,] creatures, int x, int y)
        {
            var candidates = creatures[x, y];
            var aliveCandidates = candidates.ToList();
            foreach (var candidate in candidates)
                foreach (var rival in candidates)
                    if (rival != candidate && candidate.DeadInConflict(rival))
                        aliveCandidates.Remove(candidate);
            if (aliveCandidates.Count > 1)
                throw new Exception(
                    $"Creatures {aliveCandidates[0].GetType().Name} and {aliveCandidates[1].GetType().Name} claimed the same map cell");

            return aliveCandidates.FirstOrDefault();
        }

        private List<ICreature>[,] GetCandidatesPerLocation()
        {
            var creatures = new List<ICreature>[GameState.MapWidth, GameState.MapHeight];
            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                    creatures[x, y] = new List<ICreature>();
            foreach (var e in Animations)
            {
                var x = e.TargetLogicalLocation.X;
                var y = e.TargetLogicalLocation.Y;
                var nextCreature = e.Command.TransformTo ?? e.Creature;
                creatures[x, y].Add(nextCreature);
            }

            return creatures;
        }
    }
}
