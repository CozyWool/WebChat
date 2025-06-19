using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Models;

namespace WebChatApplication.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    [HttpGet("{statusCode:int}")]
    public async Task<IActionResult> StatusCodeError(int statusCode)
    {
        var model = new ErrorViewModel
                    {
                        StatusCode = statusCode
                    };
        switch (statusCode)
        {
            case 404:
                model.Title = "Страница не найдена";
                model.Message = "Страницы, которую вы ищите, не существует.";
                break;
            case 500:
                model.Title = "Внутренняя ошибка сервера";
                model.Message = "Произошла какая-то ошибка на сервере.";
                break;
            default:
                model.Title = "Произошла ошибка";
                model.Message = "";
                break;
        }

        ViewData["Title"] = model.Title;
        return View(model);
    }
}