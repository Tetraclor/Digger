using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.DataSource;

namespace WebApi.Services
{
    public class UserService
    {
        public static List<UserAppInfo> Users = new ();
        static Dictionary<string, UserAppInfo> TokenToUser = new ();

        private static User GetUserFromDbOrNull(string loginOrToken)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var userFromDb = dbContext.Users.FirstOrDefault(x => x.Name == loginOrToken || x.Token == loginOrToken);
                if(userFromDb == null)
                    return null;
                return userFromDb;
            }
        }

        private static UserAppInfo SaveLocal(User userFromDb)
        {
            if(userFromDb == null) return null;
            var userApp = new UserAppInfo() { Name = userFromDb.Name, Rate = (int)userFromDb.Rating };
            Users.Add(userApp);
            TokenToUser[userFromDb.Token ?? ""] = userApp; // Токен равне null если это бот на сервере работает зарегестрирован вручную
            return userApp;
        }

        private static UserAppInfo GetUserOrNew(string loginOrToken)
        {
            return GetUserOrNull(loginOrToken) ?? new UserAppInfo();
        }

        public static void MarkUserOnline(string login)
        {
            GetUserOrNew(login).IsUserOnline = true;
        }

        public static void MarkUserOffline(string login)
        {
            GetUserOrNew(login).IsUserOnline = false;
        }

        public static void MarkUserBotOnline(string token)
        {
            GetUserOrNew(token).IsBotOnline = true;
        }

        public static void MarkUserBotOffline(string token)
        {
            GetUserOrNew(token).IsBotOnline = false;
        }

        public static string GetToken(string login)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.FirstOrDefault(v => v.Name == login);
                if (user != null)
                    return user.Token;
            }
            return null;
        }

        public static UserAppInfo GetUserOrNull(string loginOrToken)
        {
            var user = Users.FirstOrDefault(v => v.Name == loginOrToken);
            user ??= TokenToUser.GetValueOrDefault(loginOrToken, null);
            user ??= SaveLocal(GetUserFromDbOrNull(loginOrToken));
            return user;
        }
    

        public static UserAppInfo AuthUser(string login, string passwrod)
        {
            var userFromDb = GetUserFromDbOrNull(login);
            if (userFromDb == null || userFromDb.Password != passwrod)
                return null;
            var userApp = SaveLocal(userFromDb);
            return userApp;
        }

        public static UserAppInfo RegisterOrNull(string login, string password)
        {
            using(var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.FirstOrDefault(v => v.Name == login);
                if (user != null) return null;
                user = new User() { Name = login, Password = password, Rating = 1500, Token = CreateToken() };
                dbContext.Add(user);
                dbContext.SaveChanges();
                return SaveLocal(user);
            }
        }

        public static void SaveNewRating(params UserAppInfo[] usersApp)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                foreach(var userApp in usersApp)
                {
                    var user = dbContext.Users.FirstOrDefault(v => v.Name == userApp.Name);
                    if (user == null) 
                        continue;
                    user.Rating = (uint)userApp.Rate;
                }
                dbContext.SaveChanges();
            }
        }

        private static string CreateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
 