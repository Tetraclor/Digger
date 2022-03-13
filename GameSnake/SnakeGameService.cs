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
            var appleManager = new ApplesManager(Game, 10);
        }

        public SnakeGameService(string mapString) : base(mapString, typeof(HeadSnake))
        {
            var appleManager = new ApplesManager(Game, 10);
        }

        public override bool AddPlayer(IPlayer player)
        {
            SpawnSnake();

            this.player = player;

            return true;
        }

        private void SpawnSnake()
        {
            snake = new Snake(Game, new Point(2, 2), new Point(2, 1));
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

            if (snake.IsDead)
            {
                SpawnSnake();
            }

            snake.Move(playerMove, GameState);

            base.MakeGameTick();
        }

        public const string TestMap = @"
WWWWW WWWWWWWWWW WWWWW
W         WW         W
S         WW         S
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
W         WW         W
W         WW         W
WWW     WWWWWW     WWW
W         WW         W
W         WW         W
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
S         WW         S
W         WW         W
WWWWW WWWWWWWWWW WWWWW
";
    }
}
