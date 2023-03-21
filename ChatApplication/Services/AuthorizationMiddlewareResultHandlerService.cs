using ChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ChatApplication.Services
{
    public class AuthorizationMiddlewareResultHandlerService : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler Defaulthandler = new AuthorizationMiddlewareResultHandler();
        public async Task HandleAsync(RequestDelegate next,HttpContext context, AuthorizationPolicy policy, 
            PolicyAuthorizationResult authorizationResult)
        {
            if (authorizationResult.Challenged)
            {
                await context.Response.WriteAsJsonAsync(new ResponseWithoutData(401,"Unauthenticated: You don't have proper token to access this resource",false));
                return;
            }
            if (authorizationResult.Forbidden)
            {
                await context.Response.WriteAsJsonAsync(new ResponseWithoutData(403, "Unauthorized: You don't have permission to access this resource", false));
                return;
            }
        }
    }
}
