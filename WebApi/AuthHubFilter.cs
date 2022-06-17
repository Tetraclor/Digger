using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi
{
    public class AuthException : HubException
    {
        public AuthException(string message) : base(message)
        {
        }
    }

    public class AuthHubFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object>> next
        )
        {
            var context = invocationContext.Context.GetHttpContext();

            if (context.Request.Headers.TryGetValue("bot_token", out StringValues values))
            {
                var userName = UserService.GetUserOrNull(values.FirstOrDefault())?.Name;
                if (userName == null)
                {
                    throw new AuthException("Not found user with token");
                }
            }
            return await next(invocationContext);
        }
    }
}