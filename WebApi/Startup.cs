using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using WebApi.DataSource;
using WebApi.Services;

namespace WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Singleton);

            services.AddSingleton(new GamesManagerService());

            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddControllers();
            services.AddSignalR();
            
            services.AddDirectoryBrowser(); // Для отладки бага

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            
            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            var fileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "Images"));
            var requestPath = "/Images";
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath
            });
            
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
}
