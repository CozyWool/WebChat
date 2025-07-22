using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebChatApplication.Messages;
using WebChatApplication.Models;
using WebChatApplication.Services;

namespace WebChatApplication.Controllers;

[Route("web-chat")]
[Authorize]
public class WebChatController : Controller
{
    private readonly IChatService _chatService;
    private readonly IMessageService _messageService;

    public WebChatController(IChatService chatService, IMessageService messageService)
    {
        _chatService = chatService;
        _messageService = messageService;
    }

    [AllowAnonymous]
    [Route("/")]
    [HttpGet("index")]
    public async Task<IActionResult> Index(Guid? chatId)
    {
        ViewData["Title"] = "Веб-чат";
        if (!User.Identity.IsAuthenticated)
        {
            return View(new WebChatViewModel());
        }

        var model = await _chatService.GetChatsByUsername(User.Identity.Name, chatId);
        return View(model);
    }

    [HttpPost("private-chat/{userId:guid}")]
    public async Task<IActionResult> PrivateChat(Guid userId)
    {
        ViewData["Title"] = "Чаты";

        var model = await _chatService.GetPrivateChatByUserId(userId);
        if (model is null)
        {
            return View("_ShowStatusMessageWithButtons",
                        new StatusMessageModel("Произошла ошибка при открытии чата", true));
        }

        return RedirectToAction("Index", "WebChat", new {chatId = model.Id});
    }

    [HttpPost("group-chat")]
    public async Task<IActionResult> GroupChat(GroupChatInfoRequest request)
    {
        ViewData["Title"] = "Чаты";
        var model = await _chatService.CreateGroupChat(request);
        if (model is null)
        {
            return View("_ShowStatusMessageWithButtons",
                        new StatusMessageModel("Произошла ошибка при открытии чата", true));
        }

        return RedirectToAction("Index", "WebChat", new {chatId = model.Id});
    }

    [HttpPost("add-users-to-group-chat")]
    public async Task<IActionResult> AddUsersToGroupChat(Guid chatId, string userIdsJson)
    {
        try
        {
            var result = await _chatService.AddUsersToGroupChat(chatId, userIdsJson);
            return result ? Ok() : BadRequest();
        }
        catch
        {
            return BadRequest();
        }
    }
    [HttpPost("delete-user-from-group-chat")]
    public async Task<IActionResult> DeleteUserFromGroupChat(Guid chatId, Guid userId)
    {
        try
        {
            var result = await _chatService.DeleteUserFromGroupChat(chatId, userId);
            return result ? Ok() : BadRequest();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPost("load-more-messages")]
    public async Task<IActionResult> LoadMoreMessages(Guid chatId, int messageCount, int alreadyLoadedMessageCount)
    {
        var result = await _chatService.GetChatById(chatId, messageCount, alreadyLoadedMessageCount);
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         DateFormatString = "dd.MM.yyyy HH:mm:ss",
                                         DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                     };
        return new ContentResult
               {
                   Content = JsonConvert.SerializeObject(result, jsonSerializerSettings),
                   ContentType = "application/json"
               };
    }

    [HttpPost("save-chat-info-changes")]
    public async Task<IActionResult> SaveChatInfoChanges(GroupChatInfoRequest request)
    {
        try
        {
            var result = await _chatService.UpdateGroupChat(request);
            return result ? Ok() : BadRequest();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpDelete("delete-message/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (await _messageService.Delete(id))
        {
            return Ok();
        }

        return BadRequest();
    }
}