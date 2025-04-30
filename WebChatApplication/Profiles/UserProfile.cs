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
        CreateMap<UserEntity, UserCardModel>().AfterMap<UserEntityToUserCardModelMappingAction>();
        CreateMap<UserAndRelationTypeRecord, UserCardModel>().AfterMap<UserAndRelationTypeRecordToUserCardModelMappingAction>();
    }

    private class UserAndRelationTypeRecordToUserCardModelMappingAction(IUserService userService)
        : IMappingAction<UserAndRelationTypeRecord, UserCardModel>
    {
        public void Process(UserAndRelationTypeRecord source, UserCardModel destination, ResolutionContext context)
        {
            destination.ProfilePictureUrl = userService.GetProfilePictureUrl(source.ToUser.Username).Result;
            destination.UserId = source.ToUser.Id;
            destination.Username = source.ToUser.Username;
            destination.LastActivityAt = source.ToUser.LastActivityAt;
            destination.IsOnline = userService.IsOnline(source.ToUser.LastActivityAt);
        }
    }
    private class UserEntityToUserCardModelMappingAction(IUserService userService)
        : IMappingAction<UserEntity, UserCardModel>
    {
        public void Process(UserEntity source, UserCardModel destination, ResolutionContext context)
        {
            destination.ProfilePictureUrl = userService.GetProfilePictureUrl(source.Username).Result;
            destination.UserId = source.Id;
            destination.Username = source.Username;
            destination.LastActivityAt = source.LastActivityAt;
            destination.IsOnline = userService.IsOnline(source.LastActivityAt);
        }
    }
}