using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Common
{
    /// <summary>
    /// Клиент
    /// 1. Установка соединения (передача токена, url сервера)
    /// 2. Регистрация обработчика тика (пока одной игровй сессии, как подключать сразу несколько посмотрим)
    /// </summary>
    public class ClientSignalR
    {
        public string ServerUrl { get; set; } = "https://localhost:5001/";

        HubConnection connection;

        const string OnMethodName = "Receive";
        const string SendMethodName = "SendTurn";


        public ClientSignalR(string gameId, string token)
        {
            connection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(ServerUrl, opt => opt.AccessTokenProvider = async () => token)
                .Build();
        }

        public async void StartAsync(IPlayer player)
        {
            connection.On<ISnakeGameStateForPlayer>(OnMethodName, Tick);

            await connection.StartAsync();

            void Tick(ISnakeGameStateForPlayer gameState)
            {
                var command = player.GetCommand(gameState);
                connection.SendAsync(SendMethodName, command);
            }
        }
    }
}
