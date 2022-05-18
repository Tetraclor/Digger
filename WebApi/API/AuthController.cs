using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi.API
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [Route("assembly")]
        public IActionResult AssemblyDate()
        {
            var buildDateTime = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            return Ok($"Build time Тест Гость: {buildDateTime.ToShortDateString()} {buildDateTime.ToShortTimeString()}");
        }

        [Route("login")]
        public async Task<ActionResult> Login([FromServices]IHubContext<MainHub> hubContext, [FromQuery]string login, [FromQuery]string password)
        {
            if(login == null) return this.BadRequest("Логин не должен быть пустой");

            var user = UserService.AuthUser(login, password);
            if (user == null) return this.BadRequest("Логини или пароль неверные");
            
            await Auth(login, password);

            return this.Ok();
        }

        [Route("reg")]
        public async Task<ActionResult> Reg([FromQuery]string login, [FromQuery]string password)
        {
            if (login == null) return this.BadRequest("Логин не должен быть пустой");

            var user = UserService.RegisterOrNull(login, password);
            if(user == null) return this.BadRequest("Логин занят");

            await Auth(login, password);

            return Ok();
        }

        private async Task Auth(string login, string password)
        {
            var claimsIdentity = new ClaimsIdentity("Auth");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, login));
            claimsIdentity.AddClaim(new Claim("Password", password));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(claimsPrincipal);
        }
    }
}
