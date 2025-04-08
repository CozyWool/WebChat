using FluentEmail.Core;
using WebChatApplication.Models.User.Email;

namespace WebChatApplication.Services;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public EmailService(IFluentEmail fluentEmail)
    {
        _fluentEmail = fluentEmail;
    }
    public async Task SendUsingTemplateFromFileAsync<T>(EmailMetadata emailMetadata, T model, string templateFile)
    {
        await _fluentEmail
            .To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .UsingTemplateFromFile(templateFile, model)
            .SendAsync();
    }
}

