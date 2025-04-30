using WebChatApplication.Enums;
using WebChatApplication.Messages;
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
    Task<UserServiceStatusCodes> UpdateProfile(ChangeProfileInfoModel model);
    Task<UserServiceStatusCodes> UpdateEmail(ChangeEmailModel model);
    Task<UserServiceStatusCodes> UpdatePassword(ChangePasswordModel model);
    Task<UserServiceStatusCodes> UpdatePassword(string emailOrUsername, string newPassword);
    Task<UserServiceStatusCodes> VerifyPassword(string usernameOrLogin, string password);
    Task<UserServiceStatusCodes> DeleteUser(string name);
    Task<UserServiceStatusCodes> ConfirmEmail(string email, string token);
    Task<UserServiceStatusCodes> SendConfirmationEmail(EmailUserModel emailModel);
    Task<UserServiceStatusCodes> SendPasswordRecoveryEmail(SendPasswordRecoveryEmailModel model);
    Task<UserServiceStatusCodes> VerifyPasswordRecoveryToken(string email, string token);
/*
    Task<UserServiceStatusCodes> ReAuthenticate(string? emailOrUsername);
*/
    Task<ProfileInfoModel?> GetUserProfile(string profileUsername);
    Task<string> GetProfilePictureUrl(string username);
    Task<bool> DeleteProfilePicture(string username);
    Task<(List<UserCardModel> items, int count)> GetUsersPagedSortedFiltered(FindUsersRequest request);

    Task<(List<UserCardModel> items, int count)> GetUsersPagedSortedFilteredByMultipleRoles(
        List<int> roles, FindUsersRequest request);

    Task<(List<UserCardModel> items, int count)> GetUsersPagedSortedFilteredByMultipleRelationTypes(
        List<UserRelationTypes> relationTypes, FindUsersRequest request);

/*
    Task<List<UserCardModel>> GetUserCards(string username, UserRelationTypes? relationType = null);
*/
    Task<UserServiceStatusCodes> UpdateLastActivity(string username);

    //TODO: Подумать, переносить ли все методы ниже в отдельный сервис IUserRelationService
    Task<UserServiceStatusCodes> SendFriendRequest(string usernameTo);
    Task<UserServiceStatusCodes> AcceptFriendRequest(string usernameTo);
    Task<UserServiceStatusCodes> CancelFriendRequest(string usernameTo);
    Task<UserServiceStatusCodes> DeleteFriend(string usernameTo);
    Task<UserServiceStatusCodes> RemoveFromBlacklist(string usernameTo);
    Task<UserServiceStatusCodes> AddToBlacklist(string usernameTo);
    bool IsOnline(DateTime? userLastActivity);
    Task<UserServiceStatusCodes> BanUser(string username);
    Task<UserServiceStatusCodes> UnbanUser(string username);
    Task<UserServiceStatusCodes> PromoteUser(string username);
    Task<UserServiceStatusCodes> DemoteUser(string username);
}