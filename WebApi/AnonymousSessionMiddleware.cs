using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.DataSource;
using WebApi.Services;

namespace WebApi
{
    public class AnonymousSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger logger;
        private readonly static HashSet<string> anonimousUserNames = new();

        public AnonymousSessionMiddleware(RequestDelegate next, ApplicationDbContext applicationContext, ILogger logger)
        {
            var litst = applicationContext.Users.ToList();
            _next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string userName;
            if (context.Request.Headers.TryGetValue("bot_token", out StringValues values))
            {
                userName = UserService.GetUserOrNull(values.FirstOrDefault())?.Name;
                if(userName == null)
                {
                    throw new Exception("Not found user with token");
                }    
            }
            else
            {
                userName = GenerateAnonimousUserName();
            }

            if (!context.User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(context.User.FindFirstValue(ClaimTypes.Name)))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userName)
                    };

                    // создаем объект ClaimsIdentity
                    var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimTypes.Name, ClaimsIdentity.DefaultRoleClaimType);
                    UserService.RegisterOrNull(userName, "");
                    // установка аутентификационных куки
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
                }
            }

            await _next(context);
        }

        private static Random random = new Random();

        private string GenerateAnonimousUserName()
        {
            logger.Information("Гость");
            logger.Information($"Гость{random.Next(100000, 999999)}");
            var name = new string($"Гость100000");
            anonimousUserNames.Add(name);
            var bytes = Encoding.Default.GetBytes(name);
            logger.Information("Create: " + name);
            name = Encoding.UTF8.GetString(bytes);
            return name;
        }
    }
}
