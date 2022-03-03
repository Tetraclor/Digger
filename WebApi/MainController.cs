using Common;
using GameCore;
using GameDigger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    [Route("api")]
    [ApiController]
    public class MainController : ControllerBase
    {
        static MainController()
        {
            var timer = new Timer(Test, null, 100, 30);

            void Test(object state)
            {
                GameService.Game.BeginAct();
                GameService.Game.EndAct();
            }
        }

        [HttpGet]
        [Route("test")]
        public string Test()
        {
            return "Ok Test";
        }

        [HttpGet]
        [Route("game_state")]
        public DtoGameState GetGameState()
        {
            var gameStateDto = new DtoGameState()
            {
                MapHeight = GameService.GameState.MapHeight,
                MapWidth = GameService.GameState.MapWidth
            };
            return gameStateDto;
        }

        static Dictionary<Guid, RemotePlayer> Players = new Dictionary<Guid, RemotePlayer>();

        [HttpGet]
        [Route("register")]
        public Guid RegisterRemotePlayer()
        {
            var guid = Guid.NewGuid();
            Players[guid] = new RemotePlayer();
            GameService.GameState.AddPlayer(Players[guid], GameService.GameState.Map[2, 1]);
            // По guid идентифицировать будем нового игрока
            return guid;
        }

        [HttpGet]
        [Route("turn")]
        public List<DtoCreatureTransformation> MakeTurn([FromQuery]Guid guid, [FromBody]PlayerCommand playerCommand)
        {
            if (Players.TryGetValue(guid, out RemotePlayer remotePlayer) == false)
                throw new BadHttpRequestException("Передан неизвестный guid. Незарегестированный игрок");

            remotePlayer.PlayerCommand = playerCommand;

            return GameService.Game.Animations.Select(v => new DtoCreatureTransformation(v)).ToList();
        }

    }

    public class RemotePlayer : IPlayer
    {
        public PlayerCommand PlayerCommand;

        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            var temp = PlayerCommand;
            // Обнуляем комнаду чтобы если удаленный пользователь не успел отправить команду не исполнялась бесконечно предыдущая команда
            PlayerCommand = new PlayerCommand();
            return temp;
        }
    }

    public static class GameService
    {
        public static Game Game;
        public static GameState GameState;

        static GameService()
        {
            var map = CreatureMapCreator.CreateMap(mapWithPlayerTerrain, Assembly.GetAssembly(typeof(Digger)));
            GameState = new GameState(map);
            Game = new Game(GameState);

           // GameState.AddPlayer(new BotPlayer(), GameState.Map[3, 2]);
        }

        private const string mapWithPlayerTerrain = @"
TTT T
TTD T
T TDT
TT TT";
    }
}
