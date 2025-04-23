using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Enums;
using WebChatApplication.Helpers;
using WebChatApplication.Messages;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Models.User.Email;
using WebChatApplication.Models.User.FilterSortPagingFriends;
using WebChatApplication.Models.User.Manage;
using WebChatApplication.Models.User.PasswordRecovery;
using WebChatApplication.Services;

namespace WebChatApplication.Controllers;

[Route("account")]
public class AccountController(IUserService userService) : Controller
{
    [HttpGet("test")]
    public IActionResult test()
    {
        return View(@"testEmailConfirmationTemplate", new EmailUserModel
        {
            Username = "CozyWool",
            Email = "vsergeev201530@gmail",
            EmailConfirmationToken = "134124124",
            EmailConfirmationUrl = "https://141241"
        });
    }

    private async Task<StatusMessageModel> SendConfirmationEmail(string? email)
    {
        if (User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value != email)
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
            new {email, token = emailToken},
            HttpContext.Request.Scheme);

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
            _ => new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту",
                true)
        };
        return statusMessage;
    }

    [HttpGet("confirm-email")]
    [Authorize]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmationModel model, string? returnUrl)
    {
        ViewData["Title"] = "Подтверждение почты";
        ViewData["ReturnUrl"] = returnUrl;
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
                "Ссылка неверна, проверьте ссылку или попробуйте выслать письмо повторно",
                true);
            return View(model);
        }

        var confirmResult = await userService.ConfirmEmail(email, token);
        model.StatusMessage = confirmResult switch
        {
            UserServiceStatusCodes.OK =>
                new StatusMessageModel("Электронная почта успешно подтверждена"),
            UserServiceStatusCodes.AlreadyEmailConfirmed =>
                new StatusMessageModel("Электронная почта уже подтверждена"),
            _ => new StatusMessageModel("Произошла ошибка при подтверждении электронной почты",
                true)
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
        ViewData["Title"] = "Вход";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var statusCode = await userService.Login(model);
        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "WebChat");
            case UserServiceStatusCodes.AccountDeleted:
                ModelState.AddModelError("", "Пользователь удалён");
                break;
            case UserServiceStatusCodes.AccountBanned:
                ModelState.AddModelError("", "Пользователь заблокирован");
                break;
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
        ViewData["Title"] = "Регистрация";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var statusCode = await userService.Register(model);
        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                return RedirectToAction("ConfirmEmail", "Account", new
                {
                    email = model.Email,
                    needToSend = true,
                    returnUrl = returnUrl
                });
            case UserServiceStatusCodes.AccountDeleted:
                ModelState.AddModelError("", "Пользователь удалён");
                break;
            case UserServiceStatusCodes.AccountBanned:
                ModelState.AddModelError("", "Пользователь забанен");
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
        {
            return RedirectToAction("Login", "Account");
        }

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

    [HttpGet("profile-info")]
    public async Task<IActionResult> ProfileInfo(string username, string? message, bool isError = false)
    {
        ViewData["ActivePage"] = nameof(ChangeProfileInfo);
        ViewData["Title"] = "Профиль";

        var model = await userService.GetUserProfile(username);
        if (model is null)
        {
            return View("_ShowStatusMessageWithButtons", new StatusMessageModel("Профиль не найден", true));
        }

        if (!string.IsNullOrEmpty(message))
        {
            var statusMessage = new StatusMessageModel(message, isError);
            model.StatusMessage = statusMessage;
        }

        return View(model);
    }

    [HttpGet("change-profile")]
    [Authorize]
    public async Task<IActionResult> ChangeProfileInfo(StatusMessageModel statusMessage = null)
    {
        ViewData["ActivePage"] = nameof(ChangeProfileInfo);
        ViewData["Title"] = "Изменение профиля";

        var username = User.Identity.Name;
        var model = new ChangeProfileInfoModel
        {
            OldUsername = username,
            Username = username,
            RoleName = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value switch
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
        return View($"Manage/{nameof(ChangeProfileInfo)}", model);
    }

    [HttpPost("change-profile")]
    [Authorize]
    public async Task<IActionResult> ChangeProfileInfo(ChangeProfileInfoModel model)
    {
        ViewData["ActivePage"] = nameof(ChangeProfileInfo);
        ViewData["Title"] = "Изменение профиля";
        if (!ModelState.IsValid)
        {
            return View($"Manage/{nameof(ChangeProfileInfo)}", model);
        }

        var statusCode = await userService.UpdateProfile(model);

        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Профиль успешно обновлён"),
            UserServiceStatusCodes.AlreadyExist =>
                new StatusMessageModel("Имя пользователя уже используется", true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден",
                true),
            _ => model.StatusMessage
        };

        return RedirectToAction("ChangeProfileInfo", model.StatusMessage);
    }

    [HttpPost("delete-profile-picture")]
    [Authorize]
    public async Task<IActionResult> DeleteProfilePicture(string username)
    {
        var result = await userService.DeleteProfilePicture(username);
        if (!result)
        {
            return Json(BadRequest());
        }

        return Json(Ok());
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
        ViewData["ActivePage"] = nameof(ChangeEmail);
        ViewData["Title"] = "Электронная почта";
        if (!ModelState.IsValid)
        {
            return View($"Manage/{nameof(ChangeEmail)}", model);
        }

        var statusCode = await userService.UpdateEmail(model);

        switch (statusCode)
        {
            case UserServiceStatusCodes.OK:
                await SendConfirmationEmail(model.NewEmail);
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
            StatusMessage = statusMessage
        };
        return View($"Manage/{nameof(ChangePassword)}", model);
    }

    [HttpPost("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        ViewData["ActivePage"] = nameof(ChangePassword);
        ViewData["Title"] = "Изменение пароля";
        if (!ModelState.IsValid)
        {
            return View($"Manage/{nameof(ChangeEmail)}", model);
        }

        var statusCode = await userService.UpdatePassword(model);

        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Пароль успешно обновлён"),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден",
                true),
            UserServiceStatusCodes.NotValid =>
                new StatusMessageModel("Текущий пароль введён неверно", true),
            UserServiceStatusCodes.AlreadyExist => new
                StatusMessageModel("Новый пароль должен отличаться от текущего",
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
        ViewData["Title"] = "Удаление аккаунта";

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
            StatusMessage = statusMessage
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
                new StatusMessageModel("Произошла ошибка при отправке письма на электронную почту",
                    true));
        }

        var passwordRecoveryToken = SecurityHelper.GenerateTokenFromEmail(email);
        var passwordRecoveryUrl = Url.Action(
            "RecoverPassword",
            "Account",
            new {email, token = passwordRecoveryToken},
            HttpContext.Request.Scheme);
        model.PasswordRecoveryToken = passwordRecoveryToken;
        model.PasswordRecoveryUrl = passwordRecoveryUrl;

        var statusCode = await userService.SendPasswordRecoveryEmail(model);
        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Письмо отправлено"),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден",
                true),
            UserServiceStatusCodes.NotValid =>
                new StatusMessageModel("Произошла ошибка при отправке письма", true),
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
            UserServiceStatusCodes.AccountBanned =>
                new StatusMessageModel("Пользователь заблокирован", true),
            UserServiceStatusCodes.AccountDeleted => new StatusMessageModel("Пользователь удалён",
                true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден",
                true),
            UserServiceStatusCodes.NotValid => new
                StatusMessageModel("Произошла ошибка при восстановлении пароля",
                    true),
            _ => model.StatusMessage
        };

        if (string.IsNullOrEmpty(statusMessage.Message))
        {
            return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);
        }

        model.StatusMessage.AddNewMessage(statusMessage);

        return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);
    }

    [HttpPost("recover-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoverPassword(PasswordRecoveryModel model)
    {
        ViewData["Title"] = "Восстановление пароля";
        if (!ModelState.IsValid)
        {
            return View($"PasswordRecovery/{nameof(RecoverPassword)}", model);
        }

        var email = model.Email;
        var token = model.PasswordRecoveryToken;

        var statusCode = await userService.VerifyPasswordRecoveryToken(email, token);
        model.StatusMessage = statusCode switch
        {
            UserServiceStatusCodes.AccountBanned =>
                new StatusMessageModel("Пользователь заблокирован", true),
            UserServiceStatusCodes.AccountDeleted => new StatusMessageModel("Пользователь удалён",
                true),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден",
                true),
            UserServiceStatusCodes.NotValid => new
                StatusMessageModel("Произошла ошибка при восстановлении пароля",
                    true),
            _ => model.StatusMessage
        };

        if (statusCode != UserServiceStatusCodes.OK)
        {
            return RedirectToAction("RecoverPassword", model.StatusMessage);
        }

        await userService.UpdatePassword(email, model.NewPassword);
        model.StatusMessage = new StatusMessageModel("Пароль успешно восстановлен");
        return View("_ShowStatusMessageWithButtons", model.StatusMessage);
    }

    //TODO: Добавить status message(обработка UserServiceStatusCodes)
    [Authorize]
    [HttpPost("send-friend-request")]
    public async Task<IActionResult> SendFriendRequest(string usernameTo, string? returnUrl = null)
    {
        await userService.SendFriendRequest(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [Authorize]
    [HttpPost("accept-friend-request")]
    public async Task<IActionResult> AcceptFriendRequest(string usernameTo, string? returnUrl = null)
    {
        await userService.AcceptFriendRequest(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [Authorize]
    [HttpPost("cancel-friend-request")]
    public async Task<IActionResult> CancelFriendRequest(string usernameTo, string? returnUrl = null)
    {
        await userService.CancelFriendRequest(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [Authorize]
    [HttpPost("delete-friend")]
    public async Task<IActionResult> DeleteFriend(string usernameTo, string? returnUrl = null)
    {
        await userService.DeleteFriend(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [Authorize]
    [HttpPost("remove-from-blacklist")]
    public async Task<IActionResult> RemoveFromBlacklist(string usernameTo, string? returnUrl = null)
    {
        await userService.RemoveFromBlacklist(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [Authorize]
    [HttpPost("add-to-blacklist")]
    public async Task<IActionResult> AddToBlacklist(string usernameTo, string? returnUrl = null)
    {
        await userService.AddToBlacklist(usernameTo);
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("ProfileInfo", "Account", new {username = usernameTo});
    }

    [HttpGet("friends")]
    [Authorize]
    public async Task<IActionResult> FriendList()
    {
        ViewData["ActivePage"] = nameof(FriendList);
        ViewData["Title"] = "Список друзей";


        var model = await userService.GetUserCards(User.Identity.Name, UserRelationTypes.Friend);
        return View($"RelatedUsers/{nameof(FriendList)}", model);
    }

    [HttpGet("incoming-friend-requests")]
    [Authorize]
    public async Task<IActionResult> IncomingFriendRequests()
    {
        ViewData["ActivePage"] = nameof(IncomingFriendRequests);
        ViewData["Title"] = "Входящие заявки";


        var model = await userService.GetUserCards(User.Identity.Name, UserRelationTypes.IncomingFriendRequest);
        return View($"RelatedUsers/{nameof(IncomingFriendRequests)}", model);
    }

    [HttpGet("outgoing-friend-requests")]
    [Authorize]
    public async Task<IActionResult> OutgoingFriendRequests()
    {
        ViewData["ActivePage"] = nameof(OutgoingFriendRequests);
        ViewData["Title"] = "Исходящие заявки";


        var model = await userService.GetUserCards(User.Identity.Name, UserRelationTypes.OutgoingFriendRequest);
        return View($"RelatedUsers/{nameof(OutgoingFriendRequests)}", model);
    }

    [HttpGet("blacklist")]
    [Authorize]
    public async Task<IActionResult> Blacklist()
    {
        ViewData["ActivePage"] = nameof(Blacklist);
        ViewData["Title"] = "Черный список";


        var model = await userService.GetUserCards(User.Identity.Name, UserRelationTypes.Blacklisted);
        model.AddRange(await userService.GetUserCards(User.Identity.Name, UserRelationTypes.BlacklistedBothWays));
        return View($"RelatedUsers/{nameof(FriendList)}", model);
    }

    [HttpGet("find-friends")]
    [Authorize]
    public async Task<IActionResult> FindFriends(FindFriendsRequest request)
    {
        ViewData["ActivePage"] = nameof(FindFriends);
        ViewData["Title"] = "Поиск друзей";

        var (users, count) = await userService.GetFindFriendsPagedSortedFiltered(request);
        var model = new FindFriendsViewModel
        {
            Users = users,
            PageViewModel = new FriendPageViewModel(count, request.PageNumber, request.PageSize),
            SortViewModel = new FriendSortViewModel(request.SortOrder),
            FilterViewModel = new FriendFilterViewModel(request.Username),
        };
        return View($"RelatedUsers/{nameof(FindFriends)}", model);
    }

    //Админская часть
    [HttpPost("ban-user")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> BanUser(string username)
    {
        var result = await userService.BanUser(username);

        var statusMessage = result switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Пользователь успешно забанен."),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден.", true),
            UserServiceStatusCodes.AccountBanned => new StatusMessageModel("Пользователь уже забанен.", true),
            _ => new StatusMessageModel()
        };

        return RedirectToAction("ProfileInfo", "Account",
            new
            {
                username = username,
                message = statusMessage.Message,
                isError = statusMessage.IsError
            });
    }

    [HttpPost("unban-user")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> UnbanUser(string username)
    {
        var result = await userService.UnbanUser(username);

        var statusMessage = result switch
        {
            UserServiceStatusCodes.OK => new StatusMessageModel("Пользователь успешно разбанен."),
            UserServiceStatusCodes.NotFound => new StatusMessageModel("Пользователь не найден.", true),
            UserServiceStatusCodes.NotValid => new StatusMessageModel("Пользователь не забанен.",true),
            _ => new StatusMessageModel()
        };

        return RedirectToAction("ProfileInfo", "Account",
            new
            {
                username = username,
                message = statusMessage.Message,
                isError = statusMessage.IsError
            });;
    }
}