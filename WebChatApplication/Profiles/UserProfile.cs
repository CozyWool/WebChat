using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;
using WebChatApplication.Models.User;
using WebChatApplication.Services;

namespace WebChatApplication.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCardModel, UserEntity>();
        CreateMap<UserAndRelationTypeRecord, UserCardModel>().AfterMap<UserCardModelMappingAction>();
    }

    private class UserCardModelMappingAction(IUserService userService)
        : IMappingAction<UserAndRelationTypeRecord, UserCardModel>
    {
        public void Process(UserAndRelationTypeRecord source, UserCardModel destination, ResolutionContext context)
        {
            destination.ProfilePictureUrl = userService.GetProfilePictureUrl(source.ToUser.Username).Result;
            destination.Username = source.ToUser.Username;
            destination.LastActivityAt = source.ToUser.LastActivityAt;
            destination.IsOnline = userService.IsOnline(source.ToUser.LastActivityAt);
        }
    }
}