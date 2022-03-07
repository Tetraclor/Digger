using Common;
using GameCore;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WinFormsGameView
{
    static class Client
    {
        static RestClient RestClient = new RestClient("http://localhost:50322");

        public static Guid RegisterPlayer()
        {
            return Get<Guid>("api/register");
        }

        public static List<CreatureTransformation> Turn(Guid guid, PlayerCommand playerCommand)
        {
            var request = new RestRequest("api/turn");
            request.AddQueryParameter("guid", guid);
            request.AddBody(playerCommand);

            var dto = RestClient.GetAsync<List<DtoCreatureTransformation>>(request).Result;
            
            return dto.Select(v => v.Map()).ToList();
        }

        public static DtoGameState GetGameState()
        {
            return Get<DtoGameState>("/api/game_state");
        }

        public static T Get<T>(string api)
        {
            var result = RestClient.GetJsonAsync<T>(api).Result;

            return result;
        }
    }
}