using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirst(ClaimTypes.Name).Value;
        }
    }
}
