using WebChatApplication.Configurations;

namespace WebChatApplication.Extensions;

public static class FluentEmailExtensions
{
    public static void AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var emailConfiguration = configuration.GetSection("EmailConfiguration")
            .Get<EmailConfiguration>();
        if (emailConfiguration == null) throw new Exception("EmailConfiguration is null");

        var from = emailConfiguration.From;
        var host = emailConfiguration.SmtpServer;
        var port = emailConfiguration.Port;
        var username = emailConfiguration.Username;
        var password = emailConfiguration.Password;

        services.AddFluentEmail(from, "WebChat")
            .AddSmtpSender(host, port, username, password)
            .AddRazorRenderer();
    }
}