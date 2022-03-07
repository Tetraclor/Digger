using System;
using System.Reflection;

namespace GameCore
{
    public abstract class GameService
    {
        public Game Game;
        public GameState GameState;
        public int CurrentTick { get; private set; }

        public GameService(int width, int height, Type gameType)
        {
            var map = CreatureMapCreator.CreateMap(width, height, Assembly.GetAssembly(gameType));
            GameState = new GameState(map);
            Game = new Game(GameState);
        }

        public GameService(string stringmap, Type gameType)
        {
            var map = CreatureMapCreator.CreateMap(stringmap, Assembly.GetAssembly(gameType));
            GameState = new GameState(map);
            Game = new Game(GameState);
        }

        public virtual void MakeGameTick()
        {
            Game.BeginAct();
            Game.EndAct();
            CurrentTick++;
        }
        public abstract bool AddPlayer(IPlayer player);
        public abstract IPlayerCommand ParsePlayerCommand(string command);
    }
}
