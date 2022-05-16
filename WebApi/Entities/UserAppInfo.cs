using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApi
{
    public class UserAppInfo
    {
        public string Name { get; set; }
        public int Rate { get; set; } = 1500;
        public bool IsUserOnline { get; set; } = false;
        public bool IsBotOnline { get; set; } = false;

        public Func<IPlayer> CreateGamePlayer = () => new RemotePlayer();
        public List<UserPlayerConnection> UserPlayerConnections = new();
    }

    public class UserPlayerConnection
    {
        public string GameId { get; set; }
        public IPlayer Player { get; set; }
        public string ConnectionId { get; set; }

        public bool IsConnected => ConnectionId != null;
    }
}
