using GameCore;

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
    }
}
