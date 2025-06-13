using WebChatApplication.Services;

namespace WebChatApplication.Middlewares;

public class LastActivityMiddleware
{
    private readonly RequestDelegate _next;

    public LastActivityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.GetEndpoint()?.DisplayName.Contains("hangfire") is true)
        {
            await _next.Invoke(context);
            return;
        }
        var user = context.User;

        if (user.Identity is {IsAuthenticated: true})
        {
            await userService.UpdateLastActivity(user.Identity.Name);
        }

        await _next.Invoke(context);
    }
}