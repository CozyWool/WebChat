using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;

namespace WebChatApplication.Profiles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<RoleModel, RoleEntity>().ReverseMap();
    }
}