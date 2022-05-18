using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
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
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Singleton);

            services.AddSingleton(new GamesManagerService());

            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddControllers();
            services.AddSignalR();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseMiddleware<AnonymousSessionMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(v => v.MapControllers());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/game");
                endpoints.MapHub<MainHub>("/main");
                endpoints.MapHub<BotsHub>("/bots");
            });
        }
    }

    public class AnonymousSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly static HashSet<string> anonimousUserNames = new();

        public AnonymousSessionMiddleware(RequestDelegate next, ApplicationDbContext applicationContext)
        {
            var litst = applicationContext.Users.ToList();
            _next = next;
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

                    // ������� ������ ClaimsIdentity
                    var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimTypes.Name, ClaimsIdentity.DefaultRoleClaimType);
                    UserService.RegisterOrNull(userName, "");
                    // ��������� ������������������ ����
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
                }
            }

            await _next(context);
        }

        private static Random random = new Random();

        private string GenerateAnonimousUserName()
        {
            var name = $"�����{random.Next(100000, 999999)}";
            anonimousUserNames.Add(name);
            var nameConverted = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(name));
            return nameConverted;
        }
    }
}
