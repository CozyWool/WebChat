namespace WebChatApplication.Enums;

public enum UserRelationTypes
{
    NotRelated,
    IncomingFriendRequest,
    OutgoingFriendRequest,
    BlacklistedByUser,
    Blacklisted,
    BlacklistedBothWays,
    Friend
}