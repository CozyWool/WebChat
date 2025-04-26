using AutoMapper;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Models;

namespace WebChatApplication.Services;

public class RoleService : IRoleService
{
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;

    public RoleService(IMapper mapper, IRoleRepository roleRepository)
    {
        _mapper = mapper;
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleModel>> GetAll() =>
        _mapper.Map<List<RoleModel>>(await _roleRepository.GetAll());

    public async Task<RoleModel?> GetByName(string name) =>
        _mapper.Map<RoleModel?>(await _roleRepository.GetByName(name));
}