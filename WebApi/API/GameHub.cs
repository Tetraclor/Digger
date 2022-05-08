using Common;
using GameCore;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DataSource;

namespace WebApi
{
    public class GameHub : Hub
    {
        static Dictionary<string, RemotePlayer> RemotePlayers = new();
        static Dictionary<string, string> ConnectionIdToGameId = new();

        IHubContext<GameHub> hubContext { get; }
        GamesHubService GamesHubService { get; }

        public GameHub(IHubContext<GameHub> hubContext, GamesHubService gamesHub)
        {
            this.GamesHubService = gamesHub;
            this.hubContext = hubContext;
        }

        public void SendTurn(string command)
        {
            if (command == null)
                return;

            if (RemotePlayers.TryGetValue(Context.ConnectionId, out RemotePlayer remotePlayer) == false)
                return;

            if (remotePlayer == null)
                return;

            remotePlayer.SetCommand(command);
        }

        public override Task OnConnectedAsync()
        {
            UserService.MarkOnline(Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            if (ConnectionIdToGameId.TryGetValue(Context.ConnectionId, out string gameId))
                GamesHubService.ExcludeFromGame(gameId, Context.UserIdentifier);
            RemotePlayers.Remove(Context.ConnectionId);
            UserService.MarkOffline(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
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

        public StartGameInfo StartGame(string gameId)
        {
            var startGameInfo = GamesHubService.GamesInfo.FirstOrDefault(v => v.GameId == gameId);

            Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            if (startGameInfo == null)
            {
                Clients.Caller.SendAsync("ShowMessage", $"Игры с таким id={gameId} не найдено");
                return null;
            }

            ConnectionIdToGameId[Context.ConnectionId] = gameId;


            if (startGameInfo.IsNotStarted)
            {
                startGameInfo.MarkAsInProgress();

                var snakeGame = new AnimateSnakeGameService
                (
                    (gameService) => null,
                    GetMap()
                );

                snakeGame.ApplesManager.SetMaxApplesCount(startGameInfo.ApplesCount);

                GamesHubService.CreateGame(gameId, snakeGame, startGameInfo.TicksToEnd);

                foreach (var playerName in startGameInfo.Players)
                {
                    var player = GamesHubService.AddPlayerToGame(gameId, playerName);

                    if (player is RemotePlayer remotePlayer)
                        RemotePlayers[Context.ConnectionId] = remotePlayer;
                }

                GamesHubService.StartGame(gameId, GameTick);
            }
            else
            {
                RemotePlayers[Context.ConnectionId] = (RemotePlayer)GamesHubService.GetPlayerOfGame(gameId, Context.UserIdentifier);
            }

            return startGameInfo;

            string GetMap()
            {
                var mapInfo = MainHub.Maps
                    .FirstOrDefault(v => v.Name == startGameInfo.MapName);
                if (mapInfo == null)
                    return SnakeGame2.SnakeGameService.TestMap;
                return mapInfo.Map;
            }
        }

        public void StopGame(string gameId)
        {
            GamesHubService.StopGame(gameId);
        }

        private void GameTick(string gameId, GameService gameService)
        {
            var playersScores = GetPlayerInfos();
            var stringMap = gameService.ToStringMap();

            hubContext.Clients.Group(gameId).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);

            gameService.MakeGameTick();

            List<PlayerInfo> GetPlayerInfos()
            {
                var snakeGame = (SnakeGame2.SnakeGameService)gameService;

                
                var playerNameToSnake = snakeGame.SnakeSpawners
                    .Where(v => v.IsActive)
                    .ToDictionary(v =>  GetUserNameFromPlayer(v.Player), v => v.SpawnedSnake);

                var playersScores = gameService.PlayersScores
                    .Select(v => new PlayerInfo(GetUserNameFromPlayer(v.Key), v.Value))
                    .ToList();

                playersScores.ForEach(v =>
                {
                    var snake = playerNameToSnake[v.Name];
                    v.SnakeHead = snake.Head;
                    v.SnakeTail = snake.AllPoints;
                });

                return playersScores;

                string GetUserNameFromPlayer(IPlayer player)
                {
                    return GamesHubService.PlayerNames.TryGetValue(player, out string name) ? name : null;
                };
            }
        }

        public AnimationInfo GetAnimateInfo()
        {
            return AnimationInfo.Instanse;
        }
    }
}
