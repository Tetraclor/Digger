using GameCore;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public interface ISnakeGameStateForPlayer : IGameStateForPlayer
    {
        Point MySnakeHead { get; }
        List<Point> MySnakeBody { get; }
        bool IsApple(Point point);
    }

    public class SnakeBot : IPlayer
    {
        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            var state = (ISnakeGameStateForPlayer)gameState;
            var move = FourDirMove.None;

            var headPoint = state.MySnakeHead;
            var targetPoint = headPoint;

            var nearApples = headPoint
               .GetNear()
               .Where(v => v.IsInBound(gameState))
               .Where(v => state.IsApple(v));

            if (nearApples.Any())
            {
                targetPoint = nearApples.FirstOrDefault();
                return ReturnCommand();
            }

            var nearFree = headPoint
                .GetNear()
                .Where(v => v.IsInBound(gameState))
                .Where(v => gameState.GetCreatureOrNull(v) == null);


            if (nearFree.Any())
            {
                targetPoint = nearFree.PickRandom();
            }

            return ReturnCommand();


            PlayerCommand ReturnCommand()
            {
                move = headPoint.ToDir(targetPoint);
                return new PlayerCommand() { Move = move };
            }
        }
    }
}
