using Common;
using GameCore;
using System;

namespace GameSnake
{
    public class SnakeGameService : GameService
    {
        Snake snake;
        IPlayer player;

        public SnakeGameService(int width, int height) : base(width, height, typeof(HeadSnake))
        {
            GameState.SetCreature(new Point(6, 6), new Apple());
            GameState.SetCreature(new Point(7, 7), new Apple());
            GameState.SetCreature(new Point(8, 8), new Apple());
        }

        public override bool AddPlayer(IPlayer player)
        {
            snake = new Snake(new Point(4, 4), new Point(4, 3), new Point(3, 3));
            snake.AddToGame(Game);

            this.player = player;

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
            var playerCommand = player.GetCommand(GameState);

            var playerMove = FourDirMove.None;
            if (playerCommand != null)
                playerMove = (playerCommand as PlayerCommand).Move;           

            snake.Move(playerMove, GameState);

            snake.Tick(GameState);

            base.MakeGameTick();
        }
    }
}
