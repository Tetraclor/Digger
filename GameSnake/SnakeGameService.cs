using Common;
using GameCore;
using System;

namespace GameSnake
{
    public class SnakeGameService : GameService
    {
        Snake snake;

        public SnakeGameService(int width, int height) : base(width, height, typeof(HeadSnake))
        {
        }

        public override bool AddPlayer(IPlayer player)
        {
            var head = new HeadSnake();
            var b1 = new BodySnake();
            var b2 = new BodySnake();

            GameState.Map[4, 4] = head;
            GameState.Map[4, 3] = b1;
            GameState.Map[3, 3] = b2;

            head.PrevCreatureCommand = new CreatureCommand() { DeltaY = 1 };
            b1.CreatureCommand = new CreatureCommand() { DeltaX = 1 };

            snake = new Snake(head, b1, b2);
            GameState.AddPlayer(player, head);

            return true;
        }

        public override IPlayerCommand ParsePlayerCommand(string command)
        {
            var move = Enum.Parse<FourDirMove>(command, true);
            var playerCommand = new PlayerCommand() { Move = move };

            return playerCommand;
        }

        public override void MakeGameTick()
        {
            snake.Tick(GameState);
            base.MakeGameTick();
        }
    }
}
