using GameCore;
using System;
using System.Collections.Generic;

namespace Common
{
    public class RandomBotPlayer : IPlayer
    {
        static Random _R = new Random();
        static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(_R.Next(v.Length));
        }

        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
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

        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            commandIndex = (commandIndex + 1) % commands.Count;
            return commands[commandIndex];
        }
    }
}
