using GameCore;
using System;
using System.Collections.Generic;

namespace GameSnake
{
    public class ApplesManager
    {
        private Random Random = new Random();
        private Game game;
        public HashSet<Apple> apples = new HashSet<Apple>();

        public ApplesManager(Game game, int applesCount)
        {
            this.game = game;

            for (int i = 0; i < applesCount; i++)
            {
                CreateRandomApple();
            }
        }

        public ApplesManager(Game game, params Point[] apples)
        {
            this.game = game;

            foreach (var p in apples)
            {
                CreateApple(p);
            }
        }

        public void AppleDead(Apple apple)
        {
            if (apples.Contains(apple) == false) return;
            apples.Remove(apple);
            CreateRandomApple();
        }

        public void CreateRandomApple()
        {
            var x = Random.Next(game.GameState.MapWidth - 1);
            var y = Random.Next(game.GameState.MapWidth - 1);

            var p = new Point(x, y);

            CreateApple(p);
        }

        public void CreateApple(Point point)
        {
            var creature = game.GameState.GetCreatureOrNull(point);
            if (creature != null)
            {
                return;
            }
            var newApple = new Apple(this);
            apples.Add(newApple);
            game.GameState.SetCreature(point, newApple);
        }
    }

    public class Apple : ICreature
    {
        public ApplesManager ApplesManager;

        public Apple(ApplesManager manager)
        {
            this.ApplesManager = manager;
        }

        public CreatureCommand Act(GameState game, int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int TransformPriority()
        {
            return 2;
        }
    }
}
