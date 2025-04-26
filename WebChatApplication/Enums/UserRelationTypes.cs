using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Enums;

public enum UserRelationTypes
{
    [Display(Name = "Не связаны")] NotRelated,

    [Display(Name = "Входящие заявки в друзья")]
    IncomingFriendRequest,

    [Display(Name = "Исходящие заявки в друзья")]
    OutgoingFriendRequest,
    [Display(Name = "Добавили в черный список вас")] BlacklistedByUser,
    [Display(Name = "Добавленные в черный список")] Blacklisted,

    [Display(Name = "Добавили в черный список вас и вы добавили в черный список")]
    BlacklistedBothWays,
    [Display(Name = "Друзья")] Friend
}