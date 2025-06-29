namespace WebChatApplication.Messages;

public class GroupChatInfoRequest
{
    public Guid ChatId { get; set; }
    public string ChatName { get; set; }
    public IFormFile? ChatPicture { get; set; }
    public string UserIdsJson { get; set; }
}