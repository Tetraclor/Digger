using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace BotsTester
{

    public class ClientSignalR
    {
        public static string LocalUrl = "https://localhost:5001";
        public static string RemoteUrl = "http://www.snakearena.online";

        public string ServerUrl { get; }

        HubConnection connection;

        const string OnMethodName = "Receive";
        const string SendMethodName = "SendTurn";
        const string JoinToGameMethodName = "JoinToGame";
        private readonly string token;
        private readonly Func<IPlayer> createPlayer;
        public Dictionary<string, HubConnection> GameConnections = new();

        public bool IsConnected => connection.State == HubConnectionState.Connected || 
             GameConnections.Any(v => v.Value.State == HubConnectionState.Connected);

        public ClientSignalR(string url, string token, Func<IPlayer> createPlayer)
        {
            ServerUrl = url.Trim('/', '\\');
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
            try
            {
                await connection.StartAsync();
                Console.WriteLine($"Соединение установлено c {ServerUrl}");
                await connection.SendAsync("Join", token);
                Console.WriteLine("Бот успешно подключен");
            }catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            await connection.StopAsync();
            foreach(var (gameId, gameConnection) in GameConnections)
            {
                await gameConnection.StopAsync();
                Console.WriteLine($"Соединение с игрой {gameId} заверешено");
            }
            Console.WriteLine($"Соединение с хабом ботов заверешено");
        }

        async Task JoinToGameAsync(string gameId)
        {
            Console.WriteLine($"Бот вызван на дуэль {ServerUrl}.html/game?game_id={gameId}");

            var gameConnection = new HubConnectionBuilder()
                .WithUrl($"{ServerUrl}/game", opt => {
                    opt.Headers.Add("bot_token", token);
                })
                .Build();

            var player = createPlayer();

            gameConnection.On<GameStateView>(OnMethodName, Tick);
            await gameConnection.StartAsync();
            Console.Write($"Соединение с игрой {gameId} установлено. ");
            await gameConnection.SendAsync("StartGame", gameId);
            Console.WriteLine($"Бот успешно присоединился к игре {gameId}");

            GameConnections[gameId] = gameConnection;

            async void Tick(GameStateView gameState)
            {
                gameState.MyPlayerName = "TestUser";
                var command = Enum.GetName(((PlayerCommand)player.GetCommand(gameState)).Move);
                await gameConnection.SendAsync(SendMethodName, command);
            }
        }

        public class GameStateView : ISnakeGameStateForPlayer
        {
            public string MyPlayerName;
            public string Map { get; set; }
            public int Tick { get; set; }
            public List<PlayerInfo> PlayersScores { get; set; }
            public SnakeGameStateDto GameState { get; set; }

            public SnakeGameStateDto.SnakeDto MySnake => GameState.Snakes.FirstOrDefault(v => v.PlayerOwner.Name == MyPlayerName);

            public Point MySnakeHead => MySnake.HeadPosition;

            public List<Point> MySnakeBody => MySnake.BodyPositions;

            public int MapWidth => throw new NotImplementedException();

            public int MapHeight => throw new NotImplementedException();

            public ICreature GetCreatureOrNull(Point point)
            {
                throw new NotImplementedException();
            }

            public bool IsApple(Point point)
            {
                return GameState.ApplesPositions.Contains(point);
            }
        }

        /// <summary>
        /// Состояние игры в виде объектов, общее для всех игроков
        /// </summary>
        public class SnakeGameStateDto
        {

            // Игровая информация
            // Змейки, спавнеры, стены, яблоки
            public List<Point> ApplesPositions { get; set; }
            public List<Point> WallsPositions { get; set; }
            public List<Point> SpawnersPositions { get; set; }
            public List<SnakeDto> Snakes { get; set; }

            public class SnakeDto
            {
                public PlayerDto PlayerOwner { get; set; }
                public Point HeadPosition { get; set; }
                public List<Point> BodyPositions { get; set; }
            }

            public class PlayerDto
            {
                public string Name { get; set; }
                public int Score { get; set; }
                public Point SpawnerPosition { get; set; }
            }
        }

        public class PlayerInfo
        {
            public string Name { get; set; }
            public int Score { get; set; }
            public Point SnakeHead { get; set; }
            public List<Point> SnakeTail { get; set; }
        }
    }
}
