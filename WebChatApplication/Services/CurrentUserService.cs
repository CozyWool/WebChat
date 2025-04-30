using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;

namespace WebChatApplication.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public Guid? CurrentUserId
    {
        get
        {
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var result = Guid.TryParse(_httpContextAccessor
                                       .HttpContext
                                       ?.User
                                       .Claims
                                       .FirstOrDefault(c => c.Type == "UserId")
                                       ?.Value,
                                       out var id);
            return result ? id : null;
        }
    }

    public async Task<UserEntity?> GetCurrentUser()
    {
        if (CurrentUserId is null)
        {
            return null;
        }

        var user = await _userRepository.GetById(CurrentUserId.Value);
        return user;
    }
}