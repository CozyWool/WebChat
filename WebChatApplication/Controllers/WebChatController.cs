using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Models;
using WebChatApplication.Services;

namespace WebChatApplication.Controllers;

[Route("web-chat")]
[Authorize]
public class WebChatController : Controller
{
    private readonly IChatService _chatService;

    public WebChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [AllowAnonymous]
    [Route("/")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Веб-чат";

        return View();
    }

    [Route("private-chat/{userId:guid}")]
    public async Task<IActionResult> PrivateChat(Guid userId)
    {
        ViewData["Title"] = "Чаты";

        var model = await _chatService.GetPrivateChatByUserId(userId);
        if (model is null)
        {
            return View("_ShowStatusMessageWithButtons",
                        new StatusMessageModel("Произошла ошибка при открытии чата", true));
        }

        return View(model);
    }
}