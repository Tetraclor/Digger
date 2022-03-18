using GameCore;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common
{
    public abstract class GameService
    {
        public Game Game;
        public GameState GameState;
        public int CurrentTick { get; protected set; }
        public Dictionary<IPlayer, int> PlayersScores { get; protected set; } = new ();

        public GameService(int width, int height, Type gameType)
        {
            var map = CreatureMapCreator.CreateMap(width, height, Assembly.GetAssembly(gameType));
            GameState = new GameState(map);
            Game = new Game(GameState);
        }

        public GameService(string stringmap, Type gameType)
        {
            Init(stringmap, gameType);
        }

        public GameService()
        {
        }

        protected void Init(string stringmap, Type gameType)
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

        public virtual string ToStringMap()
        {
            return CreatureMapCreator.MapToString(this.GameState.Map);
        }

        public virtual List<CreatureTransformation> GetCreatureTransformations()
        {
            return Game.Transformations;
        }

        public virtual bool AddPlayer(IPlayer player) => PlayersScores.TryAdd(player, 0);
        public abstract bool RemovePlayer(IPlayer player);
        public virtual int GetScore(IPlayer player) => PlayersScores.TryGetValue(player, out int score)  ? score : 0;

        public virtual IPlayerCommand ParsePlayerCommand(string command)
        {
            var move = Enum.Parse<FourDirMove>(command, true);
            var playerCommand = new PlayerCommand() { Move = move };

            return playerCommand;
        }
    }

    public interface IPlayer
    {
        IPlayerCommand GetCommand(IGameStateForPlayer gameState);
    }

    public interface IPlayerCommand
    {
    }

    public interface IGameStateForPlayer
    {
        int MapWidth { get; }
        int MapHeight { get; }
        ICreature GetCreatureOrNull(Point point);
    }
}
