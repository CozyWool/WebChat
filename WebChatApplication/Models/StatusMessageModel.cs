namespace WebChatApplication.Models;

public class StatusMessageModel(string message, bool isError = false)
{
    public StatusMessageModel() : this("")
    {
    }

    public string Message { get; set; } = message;
    public bool IsError { get; set; } = isError;

    public void AddNewMessage(StatusMessageModel statusMessage)
    {
        Message = $"{Message}; " +
                  $"{statusMessage.Message}";
        IsError = statusMessage.IsError || IsError;
    }
}