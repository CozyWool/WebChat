using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IUserRelationRepository
{
    Task<List<UserRelationEntity>> GetAll();
    Task<List<UserRelationEntity>> GetAllByFromId(Guid fromUserId);
    Task<List<UserRelationEntity>> GetAllByToId(Guid toUserId);
    Task<UserRelationEntity?> GetById(Guid fromUserId, Guid toUserId);
    Task Create(UserRelationEntity? entity);
    Task Update(UserRelationEntity entity);
    Task<bool> Delete(Guid fromId, Guid toId);
}