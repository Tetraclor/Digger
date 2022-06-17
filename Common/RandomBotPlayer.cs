using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }

    public class RandomBotPlayer : IPlayer
    {
        Random _R;

        public RandomBotPlayer(int seed)
        {
            _R = new Random(seed);
        }

        T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(_R.Next(v.Length));
        }

        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            return new PlayerCommand() { Move = RandomEnumValue<FourDirMove>() };
        }
    }

    public class ListBotPlayer : IPlayer
    {
        public static ListBotPlayer DownLeft = new ListBotPlayer(FourDirMove.Down, FourDirMove.Left);

        private List<IPlayerCommand> commands = new List<IPlayerCommand>();
        private int commandIndex = -1;

        public ListBotPlayer(List<IPlayerCommand> commands)
        {
            this.commands = commands;
        }

        public ListBotPlayer(params FourDirMove[] moves)
        {
            foreach (var item in moves)
            {
                commands.Add(new PlayerCommand() { Move = item});
            }
        }

        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            commandIndex = (commandIndex + 1) % commands.Count;
            return commands[commandIndex];
        }
    }

    public class MyBot : IPlayer
    {
        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            throw new NotImplementedException();

            /// Кодите здесь пишите логику
        }
    }
}
