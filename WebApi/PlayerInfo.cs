using Common;
using System;
using System.Collections.Generic;

namespace WebApi
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int Rate { get; set; } = 1500;
        public Func<IPlayer> CreateGamePlayer = () => new RemotePlayer();
        public List<string> IdGamesIsMember = new(); // Ид игр где является участником
        public List<IPlayer> GamesPlayers = new(); // 

        public IPlayer JoinGame(string gameId)
        {
            var player = CreateGamePlayer();
            IdGamesIsMember.Add(gameId);
            GamesPlayers.Add(player);
            return player;
        }

        public IPlayer GetPlayerFromGame(string gameId)
        {
            var index = IdGamesIsMember.IndexOf(gameId);
            if (index == -1) return null;
            return GamesPlayers[index];
        }

        public void ExcludeFromGame(string gameId)
        {
            var index = IdGamesIsMember.IndexOf(gameId);
            if (index == -1) return;
            IdGamesIsMember.RemoveAt(index);
            GamesPlayers.RemoveAt(index);
        }
    }
}
