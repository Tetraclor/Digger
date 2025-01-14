﻿using GameCore;
using System;
using System.Collections.Generic;

namespace SnakeGame2
{
    public class ApplesManager : IMapAble
    {
        private Random Random = new Random();
        private GameBoard gameState;
        public int MaxApplesCount { get; private set; }
        public HashSet<Point> apples = new HashSet<Point>();

        public ApplesManager(GameBoard gameState, int applesCount)
        {
            MaxApplesCount = applesCount;
            this.gameState = gameState;

            for (int i = 0; i < applesCount; i++)
            {
                CreateRandomApple();
            }
        }

        public ApplesManager(GameBoard gameState, params Point[] apples)
        {
            this.gameState = gameState;

            foreach (var p in apples)
            {
                CreateApple(p);
            }
        }

        public void SetMaxApplesCount(int applesCount)
        {
            MaxApplesCount = applesCount;
            apples.Clear();
            for (int i = 0; i < applesCount; i++)
            {
                CreateRandomApple();
            }
        }

        public void AppleDead(Point apple)
        {
            if (apples.Contains(apple) == false) return;
            apples.Remove(apple);

            if(apples.Count < MaxApplesCount)
                CreateRandomApple();
        }

        public void CreateRandomApple()
        {
            var x = Random.Next(gameState.MapWidth - 1);
            var y = Random.Next(gameState.MapHeight - 1);

            var p = new Point(x, y);

            CreateApple(p);
        }

        public void CreateApple(Point point)
        {
            var creature = gameState.GetCreatureOrNull(point);
            if (creature != null)
            {
                return;
            }
            apples.Add(point);
        }

        public void SetToMap(Action<Point, ICreature> set)
        {
            foreach (var item in apples)
            {
                set(item, new Apple());
            }
        }
    }
}
