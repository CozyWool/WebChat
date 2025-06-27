using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Enums;
using WebChatApplication.Models.User;

namespace WebChatApplication.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private IMapper _mapper;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _mapper = mapper;
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

    public async Task<List<UserCardModel>> GetFriends()
    {
        var currentUser = await GetCurrentUser();
        if (currentUser is null)
        {
            return [];
        }
        var friends = currentUser
                      .RelatedUsers
                      .Where(x => x.RelationType == UserRelationTypes.Friend)
                      .Select(x => x.ToUser)
                      .ToList();
        return _mapper.Map<List<UserCardModel>>(friends);
    }
}