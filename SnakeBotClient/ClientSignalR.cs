using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameCore;
using Microsoft.AspNetCore.SignalR.Client;
using SnakeGame2;

namespace SnakeBotClient
{

    public class ClientSignalR
    {
        public string ServerUrl { get; set; } = "https://localhost:5001";

        HubConnection connection;

        const string OnMethodName = "Receive";
        const string SendMethodName = "SendTurn";
        const string JoinToGameMethodName = "JoinToGame";
        private readonly string token;
        private readonly Func<IPlayer> createPlayer;

        public ClientSignalR(string token, Func<IPlayer> createPlayer)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{ServerUrl}/bots", opt => {
                    opt.Headers.Add("bot_token", token);
                })
                .Build();

            connection.On<string>(JoinToGameMethodName, JoinToGameAsync);

            this.token = token;
            this.createPlayer = createPlayer;
        }

        public async Task StartAsync()
        {
            await connection.StartAsync();
            await connection.SendAsync("Join", token);
        }

        async Task JoinToGameAsync(string gameId)
        {
            var gameConnection = new HubConnectionBuilder()
                .WithUrl($"{ServerUrl}/game")
                .Build();

            var player = createPlayer();

            gameConnection.On<string, int, List<PlayerInfo>>(OnMethodName, Tick);
            await gameConnection.StartAsync();
            await gameConnection.SendAsync("StartGame", gameId);

            async void Tick(string map, int tick, List<PlayerInfo> playerInfos)
            {
                var gameState = new GameState(map, playerInfos);
                var command = Enum.GetName<FourDirMove>(((PlayerCommand)player.GetCommand(gameState)).Move);
                await gameConnection.SendAsync(SendMethodName, command);
            }
        }


        class GameState : ISnakeGameStateForPlayer
        {
            private readonly PlayerInfo myPlayer;
            private readonly string map;
            private readonly List<PlayerInfo> playerInfos;
            private readonly ICreature[,] creaturesMap;

            public GameState(string map, List<PlayerInfo> playerInfos)
            {
                this.myPlayer = playerInfos[0];
                this.map = map;
                this.playerInfos = playerInfos;
                //this.creaturesMap = CreatureMapCreator.CreateMap(map, typeof(SnakeGame2.Apple).Assembly);
            }

            public int MapWidth => creaturesMap.GetLength(0);

            public int MapHeight => creaturesMap.GetLength(1);

            public Point MySnakeHead => myPlayer.SnakeHead;

            public List<Point> MySnakeBody => myPlayer.SnakeTail;

            public ICreature GetCreatureOrNull(Point point)
            {
                return creaturesMap[point.X, point.Y];
            }

            public bool IsApple(Point point)
            {
                return GetCreatureOrNull(point) is Apple;
            }
        }

        class PlayerInfo
        {
            public string Name { get; set; }
            public int Score { get; set; }
            public Point SnakeHead { get; set; }
            public List<Point> SnakeTail { get; set; }

            public PlayerInfo()
            {
            }

            public PlayerInfo(string name, int score)
            {
                Name = name;
                Score = score;
            }
        }

    }
}
