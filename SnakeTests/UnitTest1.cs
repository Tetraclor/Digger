using Common;
using GameSnake;
using NUnit.Framework;

namespace SnakeTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var gameService = new SnakeGameService(5, 5);
            var remotePlayer = new RemotePlayer();
            
            gameService.AddPlayer(remotePlayer);
            gameService.MakeGameTick();
            gameService.MakeGameTick();
        }
    }
}