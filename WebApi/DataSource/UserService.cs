using System.Collections.Generic;
using System.Linq;

namespace WebApi.DataSource
{
    public class UserService
    {
        static HashSet<string>  OnlineUsers = new HashSet<string>();

        public static bool IsOnline(string login)
        {
            return OnlineUsers.Contains(login);
        }

        public static void MarkOnline(string login)
        {
            OnlineUsers.Add(login);
        }

        public static void MarkOffline(string login)
        {
            OnlineUsers.Remove(login);
        }

        public static List<User> GetAllUser()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                return dbContext.Users.ToList();
            }
        }

        public static User GetOrNull(string login, string passwrod)
        {
            using(var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.FirstOrDefault(v => v.Name == login);
                if (user != null && user.Password == passwrod)
                    return user;
            }
            return null;
        }

        public static User RegisterOrNull(string login, string password)
        {
            using(var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.FirstOrDefault(v => v.Name == login);
                if (user != null) return null;
                user = new User() { Name = login, Password = password, Rating = 1500 };
                dbContext.Add(user);
                dbContext.SaveChanges();
                return user;
            }
        }
    }
}
