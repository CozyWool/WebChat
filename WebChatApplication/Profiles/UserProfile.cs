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
    }
    private class UserEntityToUserCardModelMappingAction(IUserService userService, ICurrentUserService currentUserService)
        : IMappingAction<UserEntity, UserCardModel>
    {
        public void Process(UserEntity source, UserCardModel destination, ResolutionContext context)
        {
            destination.ProfilePictureUrl = userService.GetProfilePictureUrl(source.Username).Result;
            destination.UserId = source.Id;
            destination.Username = source.Username;
            destination.LastActivityAt = source.LastActivityAt;
            destination.IsOnline = userService.IsOnline(source.LastActivityAt);
            destination.RelationType = UserRelationTypes.NotRelated;
         
            var currentUser =  currentUserService.GetCurrentUser().Result;
            var relatedUser = currentUser?.RelatedUsers
                                         .FirstOrDefault(x => x.ToUser.Username == source.Username);
            if (relatedUser is not null)
            {
                destination.RelationType = relatedUser.RelationType;
            }
        }
    }
}