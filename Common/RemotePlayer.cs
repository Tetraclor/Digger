using GameCore;
using System;

namespace Common
{
    public class RemotePlayer : IPlayer
    {
        public IPlayerCommand PlayerCommand;

        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            var temp = PlayerCommand;
            // Обнуляем комнаду чтобы если удаленный пользователь не успел отправить команду не исполнялась бесконечно предыдущая команда
            PlayerCommand = null;
            return temp;
        }

        public void SetCommand(string command)
        {
            PlayerCommand = Parse(command);
        }

        public static IPlayerCommand Parse(string command)
        {
            var move = Enum.Parse<FourDirMove>(command, true);
            var playerCommand = new PlayerCommand() { Move = move };

            return playerCommand;
        }
    }
}
