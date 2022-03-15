using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class Game
    {
        public List<CreatureTransformation> Transformations = new List<CreatureTransformation>();
        public List<CreatureTransformation> Temp = new List<CreatureTransformation>();
        public List<ICreature>[,] Candidates;

        public bool NowEndAct = false;
        public bool NowBeginAct = false;

        public GameState GameState { get; }

        public Game(GameState gameState)
        {
            GameState = gameState;
        }

        public List<ICreature> GetCandidates(Point point)
        {
            return Candidates[point.X, point.Y].ToList();
        }

        public void CreateCreature(Point point, ICreature creature)
        {
            if (NowBeginAct)
            {
                Transformations.Add(new CreatureTransformation(point, creature));
                return;
            }
            else if(NowEndAct == false)
            {
                Temp.Add(new CreatureTransformation(point, creature));
                return;
            }
            if(CandidateDict.TryGetValue(point, out CandidatesInfo info))
            {
                info.Candidates.Add(creature);
            }
            else
            {
                var candInfo = new CandidatesInfo() { 
                    Point = point, 
                    Candidates = new List<ICreature> { creature } 
                };
                var creatureInMap = GameState.GetCreatureOrNull(point);
                if (creatureInMap != null) candInfo.Candidates.Add(creatureInMap);
                CandidateDict[point] = candInfo;
                CandidatesQueue.Enqueue(candInfo);
            }
        }

        public void DeleteCreature(ICreature creature)
        {
            if (GameState.CreaturesLocations.TryGetValue(creature, out Point location))
            if (CandidateDict.TryGetValue(location, out CandidatesInfo info))
            {
                info.Candidates.Remove(creature);
            }
            var cand = Candidates[location.X, location.Y];
            cand.Remove(creature);
            var del = Transformations.Where(v => v.Creature == creature).FirstOrDefault();
            Transformations.Remove(del);
            GameState.SetCreature(location, null);
        }

        public void DeleteCreature(Point point)
        {
            CandidateDict.Remove(point);
            GameState.SetCreature(point, null);
        }

        public void BeginAct()
        {
            NowBeginAct = true;
            GameState.CreaturesLocations.Clear();
            Transformations.Clear();

            Transformations.AddRange(Temp);
            Temp.Clear();

            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                {
                    var creature = GameState.Map[x, y];
                    if (creature == null) continue;

                    GameState.CreaturesLocations[creature] = new Point(x, y);

                    var command = creature.Act(GameState, x, y);

                    if (command == null)
                        command = new CreatureCommand();

                    if (x + command.DeltaX < 0 || x + command.DeltaX >= GameState.MapWidth || y + command.DeltaY < 0 ||
                        y + command.DeltaY >= GameState.MapHeight)
                        throw new Exception($"The object {creature.GetType()} falls out of the game field");

                    Transformations.Add(
                        new CreatureTransformation
                        {
                            Command = command,
                            Creature = creature,
                            Location = new Point(x, y),
                            TargetLogicalLocation = new Point(x + command.DeltaX, y + command.DeltaY)
                        });
                }

            Transformations = Transformations.OrderByDescending(z => z.Creature.TransformPriority()).ToList();
            NowBeginAct = false;
        }

        class CandidatesInfo
        {
            public Point Point;
            public List<ICreature> Candidates = new List<ICreature>();
        }

        Dictionary<Point, CandidatesInfo> CandidateDict = new();
        Queue<CandidatesInfo> CandidatesQueue = new();

        public void EndAct()
        {
            NowEndAct = true;
            CandidateDict.Clear();

            Candidates = GetCandidatesPerLocation();
            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                    GameState.SetCreature(new Point(x, y), SelectWinnerCandidatePerLocation(Candidates[x, y]));

            while(CandidatesQueue.Count != 0)
            {
                var info = CandidatesQueue.Dequeue();
                CandidateDict.Remove(info.Point);
                GameState.SetCreature(info.Point, SelectWinnerCandidatePerLocation(info.Candidates));
            }
            NowEndAct = false;
        }

        private List<ICreature>[,] GetCandidatesPerLocation()
        {
            var creatures = new List<ICreature>[GameState.MapWidth, GameState.MapHeight];
            for (var x = 0; x < GameState.MapWidth; x++)
                for (var y = 0; y < GameState.MapHeight; y++)
                    creatures[x, y] = new List<ICreature>();

            foreach (var e in Transformations)
            {
                var x = e.TargetLogicalLocation.X;
                var y = e.TargetLogicalLocation.Y;
                var nextCreature = e.Command.TransformTo ?? e.Creature;
                creatures[x, y].Add(nextCreature);
            }

            return creatures;
        }

        private static ICreature SelectWinnerCandidatePerLocation(List<ICreature> candidates)
        {
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

    }
}
