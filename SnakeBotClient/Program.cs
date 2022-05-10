using Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeBotClient
{
    /// <summary>
    /// Клиент
    /// 1. Установка соединения (передача токена, url сервера)
    /// 2. Регистрация обработчика тика (пока одной игровй сессии, как подключать сразу несколько посмотрим)
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var accessToken = "jeI2aTtEQkq8dPHJTK0TpA==";
            var client = new ClientSignalR(accessToken, () => new RandomBotPlayer(42));
            client.StartAsync().Wait();
            Thread.Sleep(10000000);
        }
    }
}
