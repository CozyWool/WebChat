using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Routing;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Enums;
using WebChatApplication.Helpers;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Models.User.Email;
using WebChatApplication.Models.User.Manage;
using WebChatApplication.Models.User.PasswordRecovery;

namespace WebChatApplication.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated) return;

        var user = _userRepository.GetByEmailOrUsername(_httpContextAccessor.HttpContext.User.Identity.Name).Result;
        var rememberMe = _httpContextAccessor.HttpContext
            .User.HasClaim("RememberMe", true.ToString());
        Authenticate(user, rememberMe);
    }

    public async Task<UserServiceStatusCodes> Login(LoginModel model)
    {
        var user = await _userRepository.GetByEmailOrUsername(model.UsernameOrEmail, model.UsernameOrEmail);
        if (user == null)
            return UserServiceStatusCodes.NotFound;
        if (!SecurityHelper.HashMatch(model.Password, user.Id.ToString(), user.PasswordHash))
            return UserServiceStatusCodes.NotValid;
        switch (user)
        {
            case {Status: UserStatuses.Deleted}:
                return UserServiceStatusCodes.AccountDeleted;
            case {Status: UserStatuses.Banned}:
                return UserServiceStatusCodes.AccountBanned;
        }

        await Authenticate(user, model.RememberMe);
        return UserServiceStatusCodes.OK;
    }

    public async Task<bool> Logout()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return true;
    }

    public async Task<UserServiceStatusCodes> Register(RegisterModel model)
    {
        var user = await _userRepository.GetByEmailOrUsername(model.Email, model.Username);
        switch (user)
        {
            case {Status: UserStatuses.Deleted}:
                return UserServiceStatusCodes.AccountDeleted;
            case {Status: UserStatuses.Banned}:
                return UserServiceStatusCodes.AccountBanned;
        }

        if (user is not null)
            return UserServiceStatusCodes.AlreadyExist;

        var id = Guid.NewGuid();
        var hash = SecurityHelper.GenerateSaltedHash(model.Password, id.ToString());
        var userEntity = new UserEntity
        {
            Id = id,
            Email = model.Email,
            Username = model.Username,
            PasswordHash = hash,
            CreatedAt = DateTime.UtcNow,
            Status = UserStatuses.EmailNotConfirmed
        };
        await _userRepository.Create(userEntity);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> SendConfirmationEmail(EmailUserModel emailModel)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailModel.Email);
        if (user is null)
            return UserServiceStatusCodes.NotFound;
        switch (user.Status)
        {
            case UserStatuses.EmailNotConfirmed:
                break;
            case UserStatuses.Active:
                return UserServiceStatusCodes.AlreadyEmailConfirmed;
            case UserStatuses.Deleted:
                return UserServiceStatusCodes.AccountDeleted;
            case UserStatuses.Banned:
                return UserServiceStatusCodes.AccountBanned;
            default:
                return UserServiceStatusCodes.NotValid;
        }

        emailModel.Username = user.Username;

        var emailMetadata = new EmailMetadata(emailModel.Email, "Подтверждение эл. почты");
        var templateFile = $"{Directory.GetCurrentDirectory()}/EmailTemplates/EmailConfirmationTemplate.cshtml";
        await _emailService.SendUsingTemplateFromFileAsync(emailMetadata, emailModel, templateFile);

        user.EmailConfirmationToken = emailModel.EmailConfirmationToken;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> SendPasswordRecoveryEmail(SendPasswordRecoveryEmailModel model)
    {
        var user = await _userRepository.GetByEmailOrUsername(model.Email);
        if (user is null)
            return UserServiceStatusCodes.NotFound;
        switch (user.Status)
        {
            case UserStatuses.EmailNotConfirmed or UserStatuses.Active:
                break;
            case UserStatuses.Deleted:
                return UserServiceStatusCodes.AccountDeleted;
            case UserStatuses.Banned:
                return UserServiceStatusCodes.AccountBanned;
            default:
                return UserServiceStatusCodes.NotValid;
        }

        model.Username = user.Username;

        var emailMetadata = new EmailMetadata(model.Email, "Восстановление пароля");
        var templateFile = $"{Directory.GetCurrentDirectory()}/EmailTemplates/PasswordRecoveryTemplate.cshtml";
        await _emailService.SendUsingTemplateFromFileAsync(emailMetadata, model, templateFile);

        user.PasswordRecoveryToken = model.PasswordRecoveryToken;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> VerifyPasswordRecoveryToken(string? email, string? token)
    {
        var user = await _userRepository.GetByEmailOrUsername(email);
        if (user is null)
            return UserServiceStatusCodes.NotFound;

        switch (user.Status)
        {
            case UserStatuses.Deleted:
                return UserServiceStatusCodes.AccountDeleted;
            case UserStatuses.Banned:
                return UserServiceStatusCodes.AccountBanned;
        }

        if (user.PasswordRecoveryToken != token || token is null)
            return UserServiceStatusCodes.NotValid;

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> ReAuthenticate(string? emailOrUsername)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailOrUsername);
        if (user is null)
            return UserServiceStatusCodes.NotFound;
        
        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> ConfirmEmail(string email, string token)
    {
        var user = await _userRepository.GetByEmailOrUsername(email);
        if (user is null)
            return UserServiceStatusCodes.NotFound;

        switch (user.Status)
        {
            case UserStatuses.EmailNotConfirmed:
                break;
            case UserStatuses.Active:
                return UserServiceStatusCodes.AlreadyEmailConfirmed;
            case UserStatuses.Deleted:
                return UserServiceStatusCodes.AccountDeleted;
            case UserStatuses.Banned:
                return UserServiceStatusCodes.AccountBanned;
            default:
                return UserServiceStatusCodes.NotValid;
        }

        if (user.EmailConfirmationToken != token)
            return UserServiceStatusCodes.NotValid;

        user.Status = UserStatuses.Active;
        user.EmailConfirmationToken = null;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }


    public async Task<UserServiceStatusCodes> UpdateProfile(ProfileModel model)
    {
        if (model.OldUsername == model.Username) return UserServiceStatusCodes.OK;

        var user = await _userRepository.GetByEmailOrUsername(model.OldUsername);
        if (user == null) return UserServiceStatusCodes.NotFound;
        var isNewUsernameBusy = await _userRepository.GetByEmailOrUsername(model.Username) != null;
        if (isNewUsernameBusy) return UserServiceStatusCodes.AlreadyExist;

        user.Username = model.Username;
        await _userRepository.Update(user);
        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdateEmail(ChangeEmailModel model)
    {
        if (model.Email == model.NewEmail) return UserServiceStatusCodes.OK;

        var user = await _userRepository.GetByEmailOrUsername(model.Email);
        if (user is null) return UserServiceStatusCodes.NotFound;

        var userWithNewEmail = await _userRepository.GetByEmailOrUsername(model.NewEmail);
        var isNewEmailBusy = userWithNewEmail is not null;
        if (isNewEmailBusy) return UserServiceStatusCodes.AlreadyExist;

        user.Email = model.NewEmail;
        user.Status = UserStatuses.EmailNotConfirmed;
        await _userRepository.Update(user);
        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdatePassword(ChangePasswordModel model)
    {
        if (model.CurrentPassword == model.NewPassword) return UserServiceStatusCodes.AlreadyExist;

        var user = await _userRepository.GetByEmailOrUsername(_httpContextAccessor.HttpContext.User.Identity.Name);
        if (user == null) return UserServiceStatusCodes.NotFound;

        if (!SecurityHelper.HashMatch(model.CurrentPassword, user.Id.ToString(), user.PasswordHash))
            return UserServiceStatusCodes.NotValid;

        user.PasswordHash = SecurityHelper.GenerateSaltedHash(model.NewPassword, user.Id.ToString());
        user.PasswordRecoveryToken = null;

        await _userRepository.Update(user);
        //TODO: Надо бы разлогинть все существующие сессии пользователя, а для этого надо сделать механизм сессий
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdatePassword(string emailOrUsername, string newPassword)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailOrUsername);
        if (user is null) return UserServiceStatusCodes.NotFound;

        user.PasswordHash = SecurityHelper.GenerateSaltedHash(newPassword, user.Id.ToString());
        user.PasswordRecoveryToken = null;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> VerifyPassword(string usernameOrLogin, string password)
    {
        var user = await _userRepository.GetByEmailOrUsername(usernameOrLogin, usernameOrLogin);
        if (user == null) return UserServiceStatusCodes.NotFound;
        return SecurityHelper.HashMatch(password, user.Id.ToString(), user.PasswordHash)
            ? UserServiceStatusCodes.OK
            : UserServiceStatusCodes.NotValid;
    }

    public async Task<UserServiceStatusCodes> DeleteUser(string name)
    {
        if (!await _userRepository.Delete(name))
            return UserServiceStatusCodes.NotFound;
        return UserServiceStatusCodes.OK;
    }

    private async Task Authenticate(UserEntity? user, bool rememberMe)
    {
        if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("CreatedAt", user.CreatedAt.ToShortDateString()),
            new("UserId", user.Id.ToString()),
            new("RoleName", user.Role.Name),
            new("UserStatus", user.Status.ToString()),
        };
        if (rememberMe)
        {
            claims.Add(new Claim("RememberMe", rememberMe.ToString()));
        }

        var id = new ClaimsIdentity(claims,
            "ApplicationCookie",
            ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        await _httpContextAccessor.HttpContext
            .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                new AuthenticationProperties {IsPersistent = rememberMe});
    }
}