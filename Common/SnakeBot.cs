using GameCore;
using System.Linq;

namespace Common
{
    public class SnakeBot : IPlayer
    {
        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            var move = FourDirMove.None;

            //var creatures = gameState.GetPlayerCreatures(this);

            //if (creatures == null || creatures.Count == 0)
            //    return ReturnCommand();

            //var snakeHead = (dynamic)creatures[0];

            //var headPoint = (Point)snakeHead.Point;
            
            //var nearFree = headPoint
            //    .GetNear()
            //    .Where(v => v.IsInBound(gameState))
            //    .Where(v => gameState.GetCreatureOrNull(v) == null)
            //    .Select(v => headPoint.ToDir(v));

            //move = nearFree.FirstOrDefault();

            return ReturnCommand();

            PlayerCommand ReturnCommand()
            {
                return new PlayerCommand() { Move = move };
            }
        }
    }
}
