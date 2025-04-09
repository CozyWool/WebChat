using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebChatApplication.Configurations;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Enums;
using WebChatApplication.Helpers;
using WebChatApplication.Messages;
using WebChatApplication.Models.User;
using WebChatApplication.Models.User.Email;
using WebChatApplication.Models.User.Manage;
using WebChatApplication.Models.User.PasswordRecovery;

namespace WebChatApplication.Services;

public class UserService : IUserService
{
    private readonly string _bucketId;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IS3Service _s3Service;
    private readonly IUserRepository _userRepository;
    private readonly IUserRelationRepository _userRelationRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor,
                       IEmailService emailService, IS3Service s3Service, IConfiguration configuration,
                       IUserRelationRepository userRelationRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _s3Service = s3Service;
        _configuration = configuration;
        _userRelationRepository = userRelationRepository;
        _mapper = mapper;
        _bucketId = _configuration.GetSection("MinioConfiguration").Get<MinioConfiguration>().BucketId;

        if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            return;
        }

        var user = _userRepository.GetByEmailOrUsername(_httpContextAccessor.HttpContext.User.Identity.Name).Result;
        var rememberMe = _httpContextAccessor.HttpContext
                                             .User.HasClaim("RememberMe", true.ToString());
        Authenticate(user, rememberMe);
    }

    public async Task<UserServiceStatusCodes> Login(LoginModel model)
    {
        var user = await _userRepository.GetByEmailOrUsername(model.UsernameOrEmail, model.UsernameOrEmail);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        if (!SecurityHelper.HashMatch(model.Password, user.Id.ToString(), user.PasswordHash))
        {
            return UserServiceStatusCodes.NotValid;
        }

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
        {
            return UserServiceStatusCodes.AlreadyExist;
        }

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

        await Authenticate(userEntity, false);
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> SendConfirmationEmail(EmailUserModel emailModel)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailModel.Email);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

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
        {
            return UserServiceStatusCodes.NotFound;
        }

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
        {
            return UserServiceStatusCodes.NotFound;
        }

        switch (user.Status)
        {
            case UserStatuses.Deleted:
                return UserServiceStatusCodes.AccountDeleted;
            case UserStatuses.Banned:
                return UserServiceStatusCodes.AccountBanned;
        }

        if (user.PasswordRecoveryToken != token || token is null)
        {
            return UserServiceStatusCodes.NotValid;
        }

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> ReAuthenticate(string? emailOrUsername)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailOrUsername);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<ProfileInfoModel?> GetUserProfile(string profileUsername)
    {
        var currentUsername = _httpContextAccessor.HttpContext.User.Identity.Name;

        var profileUser = await _userRepository.GetByEmailOrUsername(profileUsername);
        var currentUser = await _userRepository.GetByEmailOrUsername(currentUsername);
        if (profileUser is null || currentUser is null)
        {
            return null;
        }

        var relationToUser = currentUser.RelatedUsers
                                        .FirstOrDefault(x => x.ToUserId == profileUser.Id
                                                             && x.FromUserId == currentUser.Id)
                                        ?.RelationType
                             ?? UserRelationTypes.NotRelated;


        var model = new ProfileInfoModel
                    {
                        Username = profileUsername,
                        RoleName = profileUser.Role.Name switch
                                   {
                                       "User"       => "Пользователь",
                                       "Admin"      => "Администратор",
                                       "SuperAdmin" => "Супер-Администратор",
                                       _            => "Не определена"
                                   },
                        CreatedAt = profileUser.CreatedAt,
                        LastActivityAt = profileUser.LastActivityAt,
                        IsOnline = IsOnline(profileUser.LastActivityAt),
                        ProfilePictureUrl = await GetProfilePictureUrl(profileUser.Username),
                        RelationToUser = relationToUser
                    };


        return model;
    }

    public async Task<bool> DeleteProfilePicture(string username)
    {
        if (username != _httpContextAccessor.HttpContext.User.Identity.Name)
        {
            return false;
        }

        var user = await _userRepository.GetByEmailOrUsername(username);
        if (user is null)
        {
            return false;
        }

        user.ProfilePictureFileName = null;
        await _userRepository.Update(user);
        return true;
    }

    public async Task<(List<UserCardModel> items, int count)> GetFindFriendsPagedSortedFiltered(
        FindFriendsRequest request)
    {
        var currentUsername = _httpContextAccessor.HttpContext.User.Identity.Name;
        var (users, count) = await _userRepository.GetFriendsPagedSortedFiltered(pageNumber: request.PageNumber,
                                  pageSize: request.PageSize,
                                  sortOrder: request.SortOrder,
                                  username: request.Username,
                                  currentUsername: currentUsername);


        var currentUser = await _userRepository.GetByEmailOrUsername(currentUsername);
        if (currentUser is null)
        {
            return ([], 0);
        }

        var items = new List<UserAndRelationTypeRecord>();
        foreach (var user in users)
        {
            var item = new UserAndRelationTypeRecord(user, UserRelationTypes.NotRelated);
            var relatedUser = currentUser.RelatedUsers.FirstOrDefault(x => x.ToUser.Username == user.Username);
            if (relatedUser is not null)
            {
                item.RelationType = relatedUser.RelationType;
            }

            items.Add(item);
        }

        var mappedItems = _mapper.Map<List<UserCardModel>>(items);

        return (mappedItems, count);
    }

    public async Task<List<UserCardModel>> GetUserCards(string username, UserRelationTypes? relationType = null)
    {
        var currentUser = await _userRepository.GetByEmailOrUsername(username);
        if (currentUser is null)
        {
            return [];
        }

        var query = currentUser.RelatedUsers.AsQueryable();
        if (relationType is not null)
        {
            query = query.Where(x => x.RelationType == relationType);
        }

        var items = query
                    .Select(x => new UserAndRelationTypeRecord(x.ToUser, x.RelationType))
                    .ToList();

        var mappedItems = _mapper.Map<List<UserCardModel>>(items);

        return mappedItems;
    }


    public async Task<string> GetProfilePictureUrl(string username)
    {
        var user = await _userRepository.GetByEmailOrUsername(username);
        if (user is null)
        {
            return "";
        }

        var url = await _s3Service.GetUrl(_bucketId, user.ProfilePictureFileName);
        if (string.IsNullOrEmpty(url))
        {
            url = $"https://ui-avatars.com/api/?name={user.Username}&size=200p";
        }

        return url;
    }

    public async Task<UserServiceStatusCodes> UpdateLastActivity(string username)
    {
        var user = await _userRepository.GetByEmailOrUsername(username);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        user.LastActivityAt = DateTime.UtcNow;
        await _userRepository.Update(user);
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> ConfirmEmail(string email, string token)
    {
        var user = await _userRepository.GetByEmailOrUsername(email);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

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
        {
            return UserServiceStatusCodes.NotValid;
        }

        user.Status = UserStatuses.Active;
        user.EmailConfirmationToken = null;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }


    public async Task<UserServiceStatusCodes> UpdateProfile(ChangeProfileInfoModel model)
    {
        if (model.OldUsername == model.Username && model.ProfilePicture is null)
        {
            return UserServiceStatusCodes.OK;
        }

        var user = await _userRepository.GetByEmailOrUsername(model.OldUsername);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var isNewUsernameBusy = await _userRepository.GetByEmailOrUsername(model.Username) != null &&
                                model.Username != model.OldUsername;
        if (isNewUsernameBusy)
        {
            return UserServiceStatusCodes.AlreadyExist;
        }

        user.Username = model.Username;
        if (model.ProfilePicture is not null)
        {
            var fileName = $"{user.Id}_pfp{Path.GetExtension(model.ProfilePicture.FileName)}";
            var profilePictureFileName = await _s3Service.UploadFile(_bucketId,
                                                                     fileName,
                                                                     model.ProfilePicture.OpenReadStream());
            if (!string.IsNullOrEmpty(profilePictureFileName) &&
                (user.ProfilePictureFileName is null ||
                 await _s3Service.DeleteFile(_bucketId, user.ProfilePictureFileName)))
            {
                user.ProfilePictureFileName = profilePictureFileName;
            }
        }

        await _userRepository.Update(user);
        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdateEmail(ChangeEmailModel model)
    {
        if (model.Email == model.NewEmail)
        {
            return UserServiceStatusCodes.OK;
        }

        var user = await _userRepository.GetByEmailOrUsername(model.Email);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userWithNewEmail = await _userRepository.GetByEmailOrUsername(model.NewEmail);
        var isNewEmailBusy = userWithNewEmail is not null;
        if (isNewEmailBusy)
        {
            return UserServiceStatusCodes.AlreadyExist;
        }

        user.Email = model.NewEmail;
        user.Status = UserStatuses.EmailNotConfirmed;
        await _userRepository.Update(user);
        await Authenticate(user, _httpContextAccessor.HttpContext.User.HasClaim("RememberMe", true.ToString()));
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdatePassword(ChangePasswordModel model)
    {
        if (model.CurrentPassword == model.NewPassword)
        {
            return UserServiceStatusCodes.AlreadyExist;
        }

        var user = await _userRepository.GetByEmailOrUsername(_httpContextAccessor.HttpContext.User.Identity.Name);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        if (!SecurityHelper.HashMatch(model.CurrentPassword, user.Id.ToString(), user.PasswordHash))
        {
            return UserServiceStatusCodes.NotValid;
        }

        user.PasswordHash = SecurityHelper.GenerateSaltedHash(model.NewPassword, user.Id.ToString());
        user.PasswordRecoveryToken = null;

        await _userRepository.Update(user);
        //TODO: Надо бы разлогинить все существующие сессии пользователя, а для этого надо сделать механизм сессий
        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> UpdatePassword(string emailOrUsername, string newPassword)
    {
        var user = await _userRepository.GetByEmailOrUsername(emailOrUsername);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        user.PasswordHash = SecurityHelper.GenerateSaltedHash(newPassword, user.Id.ToString());
        user.PasswordRecoveryToken = null;
        await _userRepository.Update(user);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> VerifyPassword(string usernameOrLogin, string password)
    {
        var user = await _userRepository.GetByEmailOrUsername(usernameOrLogin, usernameOrLogin);
        if (user is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        return SecurityHelper.HashMatch(password, user.Id.ToString(), user.PasswordHash)
                   ? UserServiceStatusCodes.OK
                   : UserServiceStatusCodes.NotValid;
    }

    public async Task<UserServiceStatusCodes> DeleteUser(string name)
    {
        if (!await _userRepository.Delete(name))
        {
            return UserServiceStatusCodes.NotFound;
        }

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> SendFriendRequest(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser?.RelationType is UserRelationTypes.IncomingFriendRequest &&
            userRelationToUser?.RelationType is UserRelationTypes.OutgoingFriendRequest)
        {
            // TODO: Вот тут было бы неплохо не вызывать AcceptFriendRequest(потому что опять запрос в БД приходится делать), а какой-нибудь метод для обновления типа связи
            return await AcceptFriendRequest(usernameTo);
        }

        if (userRelationFromUser is not null || userRelationToUser is not null)
        {
            return UserServiceStatusCodes.AlreadyExist;
        }

        userRelationFromUser = new UserRelationEntity
                               {
                                   FromUserId = userFrom.Id,
                                   ToUserId = userTo.Id,
                                   RelationType = UserRelationTypes.OutgoingFriendRequest
                               };
        userRelationToUser = new UserRelationEntity
                             {
                                 FromUserId = userTo.Id,
                                 ToUserId = userFrom.Id,
                                 RelationType = UserRelationTypes.IncomingFriendRequest
                             };

        await _userRelationRepository.Create(userRelationFromUser);
        await _userRelationRepository.Create(userRelationToUser);

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> AcceptFriendRequest(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser?.RelationType is not UserRelationTypes.IncomingFriendRequest ||
            userRelationToUser?.RelationType is not UserRelationTypes.OutgoingFriendRequest)
        {
            return UserServiceStatusCodes.NotValid;
        }


        userRelationFromUser.RelationType = UserRelationTypes.Friend;
        userRelationToUser.RelationType = UserRelationTypes.Friend;

        await _userRelationRepository.Update(userRelationFromUser);
        await _userRelationRepository.Update(userRelationToUser);

        return UserServiceStatusCodes.OK;
    }

    //TODO: Вынести сам процесс разрушения/изменения связи в отдельный метод?
    public async Task<UserServiceStatusCodes> CancelFriendRequest(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser?.RelationType is not UserRelationTypes.OutgoingFriendRequest ||
            userRelationToUser?.RelationType is not UserRelationTypes.IncomingFriendRequest)
        {
            return UserServiceStatusCodes.NotValid;
        }

        var deleteResult = await _userRelationRepository.Delete(userFrom.Id, userTo.Id);
        if (!deleteResult)
        {
            return UserServiceStatusCodes.NotValid;
        }

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> DeleteFriend(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser?.RelationType is not UserRelationTypes.Friend ||
            userRelationToUser?.RelationType is not UserRelationTypes.Friend)
        {
            return UserServiceStatusCodes.NotValid;
        }

        var deleteResult = await _userRelationRepository.Delete(userFrom.Id, userTo.Id);
        if (!deleteResult)
        {
            return UserServiceStatusCodes.NotValid;
        }

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> RemoveFromBlacklist(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser is null || userRelationToUser is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        if (userRelationFromUser.RelationType is UserRelationTypes.BlacklistedBothWays &&
            userRelationToUser.RelationType is UserRelationTypes.BlacklistedBothWays)
        {
            userRelationFromUser.RelationType = UserRelationTypes.BlacklistedByUser;
            userRelationToUser.RelationType = UserRelationTypes.Blacklisted;

            await _userRelationRepository.Update(userRelationFromUser);
            await _userRelationRepository.Update(userRelationToUser);
        }
        else
        {
            var deleteResult = await _userRelationRepository.Delete(userFrom.Id, userTo.Id);
            if (!deleteResult)
            {
                return UserServiceStatusCodes.NotValid;
            }
        }

        return UserServiceStatusCodes.OK;
    }

    public async Task<UserServiceStatusCodes> AddToBlacklist(string usernameTo)
    {
        var usernameFrom = _httpContextAccessor.HttpContext.User.Identity.Name;
        var userFrom = await _userRepository.GetByEmailOrUsername(usernameFrom);
        var userTo = await _userRepository.GetByEmailOrUsername(usernameTo);
        if (userFrom is null || userTo is null)
        {
            return UserServiceStatusCodes.NotFound;
        }

        var userRelationFromUser = await _userRelationRepository.GetById(userFrom.Id, userTo.Id);
        var userRelationToUser = await _userRelationRepository.GetById(userTo.Id, userFrom.Id);
        if (userRelationFromUser is null || userRelationToUser is null)
        {
            await _userRelationRepository.Delete(userFrom.Id, userTo.Id);
            await _userRelationRepository.Delete(userTo.Id, userFrom.Id);

            userRelationFromUser = new UserRelationEntity
                                   {
                                       FromUserId = userFrom.Id,
                                       ToUserId = userTo.Id,
                                       RelationType = UserRelationTypes.Blacklisted
                                   };
            userRelationToUser = new UserRelationEntity
                                 {
                                     FromUserId = userTo.Id,
                                     ToUserId = userFrom.Id,
                                     RelationType = UserRelationTypes.BlacklistedByUser
                                 };

            await _userRelationRepository.Create(userRelationFromUser);
            await _userRelationRepository.Create(userRelationToUser);
        }
        else
        {
            var oldUserFromRelation = userRelationFromUser.RelationType;
            var oldUserToRelation = userRelationToUser.RelationType;

            if (oldUserFromRelation is UserRelationTypes.BlacklistedByUser &&
                oldUserToRelation is UserRelationTypes.Blacklisted)
            {
                userRelationFromUser.RelationType = UserRelationTypes.BlacklistedBothWays;
                userRelationToUser.RelationType = UserRelationTypes.BlacklistedBothWays;
            }
            else
            {
                userRelationFromUser.RelationType = UserRelationTypes.Blacklisted;
                userRelationToUser.RelationType = UserRelationTypes.BlacklistedByUser;
            }

            await _userRelationRepository.Update(userRelationFromUser);
            await _userRelationRepository.Update(userRelationToUser);
        }


        return UserServiceStatusCodes.OK;
    }


    public bool IsOnline(DateTime? userLastActivity)
    {
        if (userLastActivity is null)
        {
            return false;
        }

        return DateTime.UtcNow - userLastActivity <= TimeSpan.FromMinutes(1);
    }

    private async Task Authenticate(UserEntity? user, bool rememberMe)
    {
        if (user is null)
        {
            return;
        }

        if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        if (_httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity existedIdentity)
        {
            var emailClaim = existedIdentity.FindFirst(ClaimTypes.Email);
            if (emailClaim?.Value != user.Email && existedIdentity.TryRemoveClaim(emailClaim))
            {
                existedIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                await _httpContextAccessor.HttpContext
                                          .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                       new ClaimsPrincipal(existedIdentity),
                                                       new AuthenticationProperties
                                                       {IsPersistent = rememberMe});
                return;
            }
        }

        var claims = new List<Claim>
                     {
                         new(ClaimTypes.Name, user.Username),
                         new(ClaimTypes.Role, user.RoleId.ToString()),
                         new(ClaimTypes.Email, user.Email),
                         new("CreatedAt", user.CreatedAt.ToShortDateString()),
                         new("UserId", user.Id.ToString()),
                         new("RoleName", user.Role.Name),
                         new("UserStatus", user.Status.ToString())
                     };
        if (rememberMe)
        {
            claims.Add(new Claim("RememberMe", rememberMe.ToString()));
        }

        var claimsIdentity = new ClaimsIdentity(claims,
                                                "ApplicationCookie",
                                                ClaimsIdentity.DefaultNameClaimType,
                                                ClaimsIdentity.DefaultRoleClaimType);

        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                           new ClaimsPrincipal(claimsIdentity),
                                                           new AuthenticationProperties
                                                           {IsPersistent = rememberMe});
    }
}