using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Enums;
using WebChatApplication.Helpers;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Models.User.Email;
using WebChatApplication.Models.User.Manage;
using WebChatApplication.Models.User.PasswordRecovery;
using WebChatApplication.Services;

namespace WebChatApplication.Controllers;

[Route("account")]
public class AccountController(IUserService userService) : Controller
{
    // Не знаю, почему Claims не обновляются сразу, поэтому приходится костылить с forceSend
    //TODO: Спросить у Александра как можно это исправить
    private async Task<StatusMessageModel> SendConfirmationEmail(string? email, bool forceSend = false)
    {
        if (!forceSend && User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value != email)
        {
            return new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту", true);
        }

        if (email is null)
        {
            return new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту", true);
        }

        var emailToken = SecurityHelper.GenerateTokenFromEmail(email);
        var emailConfirmationUrl = Url.Action(
            "ConfirmEmail",
            "Account",
            new {email = email, token = emailToken},
            protocol: HttpContext.Request.Scheme);

        var model = new EmailUserModel
        {
            Email = email,
            EmailConfirmationToken = emailToken,
            EmailConfirmationUrl = emailConfirmationUrl
        };
        var sendResult = await userService.SendConfirmationEmail(model);

        var statusMessage = sendResult switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel(
                $"Письмо для подтверждения успешно выслано на почту {email}"),
            UserServiceStatusCodes.AlreadyEmailConfirmed =>
                new StatusMessageModel("Электронная почта уже подтверждена"),
            _ => new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту", true)
        };
        return statusMessage;
    }

    [HttpGet("confirm-email")]
    [Authorize]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmationModel model)
    {
        ViewData["Title"] = "Подтверждение почты";
        var email = model.Email;
        var token = model.Token;

        if (model.NeedToSend && email is not null)
        {
            model.StatusMessage = await SendConfirmationEmail(email);
            return View("_ShowStatusMessageWithButtons", model.StatusMessage);
        }

        if (email is null || token is null)
        {
            model.StatusMessage = new StatusMessageModel(
                "Ссылка неверна, проверьте ссылку или попробуйте выслать письмо повторно", true);
            return View(model);
        }

        var confirmResult = await userService.ConfirmEmail(email, token);
        model.StatusMessage = confirmResult switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Электронная почта успешно подтверждена"),
            UserServiceStatusCodes.AlreadyEmailConfirmed =>
                new StatusMessageModel("Электронная почта уже подтверждена"),
            _ => new StatusMessageModel("Произошла ошибка при подтверждении электронной почты", true)
        };
        return View("_ShowStatusMessageWithButtons", model.StatusMessage);
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Вход";
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var statusCode = await userService.Login(model);
        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                if (!string.IsNullOrEmpty(returnUrl))
                    return LocalRedirect(returnUrl);
                return RedirectToAction("Index", "WebChat");
            case UserServiceStatusCodes.AccountDeleted:
                ModelState.AddModelError("", "Пользователь удалён");
                break;
            case UserServiceStatusCodes.AccountBanned:
                ModelState.AddModelError("", "Пользователь заблокирован");
                break;
            // TODO: Разрешить вход с неподтверждённым аккаунтом, но выводить предупреждение
            // case UserServiceStatusCodes.AccountEmailNotVerified:
            //     ModelState.AddModelError("", "Адрес эл. почты не подтверждён");
            //     break;
            case UserServiceStatusCodes.NotFound:
                ModelState.AddModelError("", "Пользователь не найден");
                break;
            case UserServiceStatusCodes.NotValid:
                ModelState.AddModelError("", "Неверный пароль или логин");
                break;
        }

        ViewData["Title"] = "Вход";
        return View(model);
    }


    [HttpGet("register")]
    public IActionResult Register(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Регистрация";
        return View();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var statusCode = await userService.Register(model);
        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                return RedirectToAction("ConfirmEmail", "Account", new
                    {email = model.Email, needToSend = true});
            case UserServiceStatusCodes.AccountDeleted:
                ModelState.AddModelError("", "Пользователь удалён");
                break;
            case UserServiceStatusCodes.AccountBanned:
                ModelState.AddModelError("", "Пользователь заблокирован");
                break;
            case UserServiceStatusCodes.AlreadyExist:
                ModelState.AddModelError("", "Пользователь уже существует");
                break;
        }

        ViewData["Title"] = "Регистрация";
        return View(model);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        ViewData["Title"] = "Выход из аккаунта";

        if (await userService.Logout())
            return RedirectToAction("Login", "Account");
        return RedirectToAction("Index", "WebChat");
    }

    [HttpGet("access-denied")]
    public async Task<IActionResult> AccessDenied(string? returnUrl)
    {
        ViewData["ErrorMessage"] = "Доступ к странице запрещён";
        ViewData["Title"] = "Отказано в доступе";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> Profile(StatusMessageModel statusMessage = null)
    {
        ViewData["ActivePage"] = nameof(Profile);
        ViewData["Title"] = "Профиль";

        var username = User.Identity.Name;
        var model = new ProfileModel
        {
            OldUsername = username,
            Username = username,
            RoleName = User.Claims.FirstOrDefault(x => x.Type == "RoleName").Value switch
            {
                "User" => "Пользователь",
                "Admin" => "Администратор",
                "SuperAdmin" => "Супер-Администратор",
                _ => "Не определена"
            },
            CreatedAt = DateTime.Parse(User.Claims.FirstOrDefault(claim => claim.Type == "CreatedAt").Value,
                CultureInfo.CurrentCulture),
            StatusMessage = statusMessage
        };
        return View($"Manage/{nameof(Profile)}", model);
    }

    [HttpPost("profile")]
    [Authorize]
    public async Task<IActionResult> Profile(ProfileModel model)
    {
        var statusCode = await userService.UpdateProfile(model);

        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Профиль успешно обновлён"),
            UserServiceStatusCodes.AlreadyExist => new StatusMessageModel("Имя пользователя уже используется", true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден", true),
            _ => model.StatusMessage
        };

        return RedirectToAction("Profile", model.StatusMessage);
    }

    [HttpGet("email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail(StatusMessageModel statusMessage = null)
    {
        ViewData["ActivePage"] = nameof(ChangeEmail);
        ViewData["Title"] = "Электронная почта";

        var email = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email).Value;
        var model = new ChangeEmailModel
        {
            Email = email,
            StatusMessage = statusMessage
        };
        return View($"Manage/{nameof(ChangeEmail)}", model);
    }

    [HttpPost("email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail(ChangeEmailModel model)
    {
        var statusCode = await userService.UpdateEmail(model);

        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                await SendConfirmationEmail(model.NewEmail, true);
                model.StatusMessage = new StatusMessageModel("Эл. почта успешно обновлена. Проверьте почту.");
                break;
            case UserServiceStatusCodes.AlreadyExist:
                model.StatusMessage = new StatusMessageModel("Эл. почта уже используется", true);
                break;
            case UserServiceStatusCodes.NotFound:
                model.StatusMessage = new StatusMessageModel("Пользователь не найден", true);
                break;
            default:
                model.StatusMessage = model.StatusMessage;
                break;
        }

        return RedirectToAction("ChangeEmail", model.StatusMessage);
    }

    [HttpGet("password")]
    [Authorize]
    public IActionResult ChangePassword(StatusMessageModel statusMessage = null)
    {
        ViewData["ActivePage"] = nameof(ChangePassword);
        ViewData["Title"] = "Изменение пароля";

        var model = new ChangePasswordModel
        {
            StatusMessage = statusMessage,
        };
        return View($"Manage/{nameof(ChangePassword)}", model);
    }

    [HttpPost("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        ViewData["ActivePage"] = nameof(ChangePassword);

        var statusCode = await userService.UpdatePassword(model);

        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Пароль успешно обновлён"),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден", true),
            UserServiceStatusCodes.NotValid => new StatusMessageModel("Текущий пароль введён неверно", true),
            UserServiceStatusCodes.AlreadyExist => new StatusMessageModel("Новый пароль должен отличаться от текущего",
                true),
            _ => model.StatusMessage
        };

        return RedirectToAction("ChangePassword", model.StatusMessage);
    }

    [HttpGet("delete")]
    [Authorize]
    public IActionResult DeleteAccount()
    {
        ViewData["ActivePage"] = nameof(DeleteAccount);
        ViewData["Title"] = "Удаление аккаунта";

        return View($"Manage/{nameof(DeleteAccount)}");
    }

    [HttpPost("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(DeleteAccountModel model)
    {
        ViewData["ActivePage"] = nameof(DeleteAccount);
        var isPasswordValid = await userService.VerifyPassword(User.Identity.Name, model.Password) ==
                              UserServiceStatusCodes.OK;

        if (!isPasswordValid)
        {
            ModelState.AddModelError("", "Неверный пароль");
            return View($"Manage/{nameof(DeleteAccount)}");
        }

        await userService.DeleteUser(User.Identity.Name);
        await userService.Logout();
        return RedirectToAction("Register");
    }

    [HttpGet("send-password-recovery")]
    [AllowAnonymous]
    public IActionResult SendPasswordRecoveryEmail(StatusMessageModel statusMessage = null)
    {
        ViewData["Title"] = "Восстановление пароля";
        var model = new SendPasswordRecoveryEmailModel
        {
            StatusMessage = statusMessage,
        };
        return View($"PasswordRecovery/{nameof(SendPasswordRecoveryEmail)}", model);
    }

    [HttpPost("send-password-recovery")]
    [AllowAnonymous]
    public async Task<IActionResult> SendPasswordRecoveryEmail(SendPasswordRecoveryEmailModel model)
    {
        var email = model.Email;
        if (email is null)
        {
            return RedirectToAction("SendPasswordRecoveryEmail",
                new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту", true));
        }

        var passwordRecoveryToken = SecurityHelper.GenerateTokenFromEmail(email);
        var passwordRecoveryUrl = Url.Action(
            "RecoverPassword",
            "Account",
            new {email = email, token = passwordRecoveryToken},
            protocol: HttpContext.Request.Scheme);
        model.PasswordRecoveryToken = passwordRecoveryToken;
        model.PasswordRecoveryUrl = passwordRecoveryUrl;

        var statusCode = await userService.SendPasswordRecoveryEmail(model);
        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Письмо отправлено"),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден", true),
            UserServiceStatusCodes.NotValid => new StatusMessageModel("Произошла ошибка при отправке письма", true),
            _ => model.StatusMessage
        };
        return RedirectToAction("SendPasswordRecoveryEmail", model.StatusMessage);
    }

    [HttpGet("recover-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoverPassword(string? email, string? token,
        StatusMessageModel? statusMessage = null)
    {
        ViewData["Title"] = "Восстановление пароля";

        var model = new PasswordRecoveryModel
        {
            Email = email,
            PasswordRecoveryToken = token
        };

        if (email is null || token is null)
        {
            model.StatusMessage =
                new StatusMessageModel("Ссылка неверна, проверьте ссылку или попробуйте выслать письмо повторно", true);
            return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);
        }

        var statusCode = await userService.VerifyPasswordRecoveryToken(email, token);
        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.AccountBanned => new StatusMessageModel("Пользователь заблокирован", true),
            UserServiceStatusCodes.AccountDeleted => new StatusMessageModel("Пользователь удалён", true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден", true),
            UserServiceStatusCodes.NotValid => new StatusMessageModel("Произошла ошибка при восстановлении пароля",
                true),
            _ => model.StatusMessage
        };

        if (string.IsNullOrEmpty(statusMessage.Message))
            return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);

        model.StatusMessage.AddNewMessage(statusMessage);

        return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);
    }

    [HttpPost("recover-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoverPassword(PasswordRecoveryModel model)
    {
        ViewData["Title"] = "Восстановление пароля";
        if (!ModelState.IsValid) return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);

        var email = model.Email;
        var token = model.PasswordRecoveryToken;

        var statusCode = await userService.VerifyPasswordRecoveryToken(email, token);
        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.AccountBanned => new StatusMessageModel("Пользователь заблокирован", true),
            UserServiceStatusCodes.AccountDeleted => new StatusMessageModel("Пользователь удалён", true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден", true),
            UserServiceStatusCodes.NotValid => new StatusMessageModel("Произошла ошибка при восстановлении пароля",
                true),
            _ => model.StatusMessage
        };

        if (statusCode != UserServiceStatusCodes.OK) return RedirectToAction("RecoverPassword", model.StatusMessage);

        await userService.UpdatePassword(email, model.NewPassword);
        model.StatusMessage = new StatusMessageModel("Пароль успешно восстановлен");
        return View("_ShowStatusMessageWithButtons", model.StatusMessage);
        // return RedirectToAction("RecoverPassword", model);
    }
}