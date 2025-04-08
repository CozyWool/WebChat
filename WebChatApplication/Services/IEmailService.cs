using WebChatApplication.Models.User.Email;

namespace WebChatApplication.Services;

public interface IEmailService
{
    Task SendUsingTemplateFromFileAsync<T>(EmailMetadata emailMetadata, T model, string templateFile);
}