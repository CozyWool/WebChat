using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebChatApplication.DataAccess.Contexts;

namespace WebChatApplication.Controllers;

[Route("web-chat")]
[Authorize]
public class WebChatController : Controller
{
    private ApplicationDbContext _applicationDbContext;

    public WebChatController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    [AllowAnonymous]
    [Route("/")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Веб-чат";

        return View();
    }
}