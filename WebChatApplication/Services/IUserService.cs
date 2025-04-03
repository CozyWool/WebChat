using WebChatApplication.Enums;
using WebChatApplication.Models.User;
using WebChatApplication.Models.User.Email;
using WebChatApplication.Models.User.Manage;
using WebChatApplication.Models.User.PasswordRecovery;

namespace WebChatApplication.Services;

public interface IUserService
{
    Task<UserServiceStatusCodes> Login(LoginModel model);
    Task<bool> Logout();
    Task<UserServiceStatusCodes> Register(RegisterModel model);
    Task<UserServiceStatusCodes> UpdateProfile(ProfileModel model);
    Task<UserServiceStatusCodes> UpdateEmail(ChangeEmailModel model);
    Task<UserServiceStatusCodes> UpdatePassword(ChangePasswordModel model);
    Task<UserServiceStatusCodes> UpdatePassword(string emailOrUsername, string newPassword);
    Task<UserServiceStatusCodes> VerifyPassword(string usernameOrLogin, string password);
    Task<UserServiceStatusCodes> DeleteUser(string name);
    Task<UserServiceStatusCodes> ConfirmEmail(string email, string token);
    Task<UserServiceStatusCodes> SendConfirmationEmail(EmailUserModel emailModel);
    Task<UserServiceStatusCodes> SendPasswordRecoveryEmail(SendPasswordRecoveryEmailModel model);
    Task<UserServiceStatusCodes> VerifyPasswordRecoveryToken(string email, string token);
    Task<UserServiceStatusCodes> ReAuthenticate(string? emailOrUsername);
}