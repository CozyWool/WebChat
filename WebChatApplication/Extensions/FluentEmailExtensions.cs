using WebChatApplication.Configurations;

namespace WebChatApplication.Extensions;

public static class FluentEmailExtensions
{
    public static void AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var emailConfiguration = configuration.GetSection("EmailConfiguration")
            .Get<EmailConfiguration>();
        var host = emailConfiguration.SmtpServer;
        var port = emailConfiguration.Port;
        var username = emailConfiguration.Username;
        var password = emailConfiguration.Password;

        services.AddFluentEmail(emailConfiguration.From, "WebChat")
            .AddSmtpSender(host, port, username, password)
            .AddRazorRenderer();
    }
}