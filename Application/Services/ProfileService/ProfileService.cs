using Application.DTOs.Profile;
using Application.Interfaces.IProfileService;
using AutoMapper;
using Dapper;
using Domain.Entities;
using Infrastructure.Interfaces;

namespace Application.Services.ProfileService;

public class ProfileService : IProfileService
{
    private readonly IGenericRepository _repo;
    private readonly IMapper _mapper;

    public ProfileService(IGenericRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<ProfileDto?> GetProfileAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        var customer = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_GetCustomerById", p);
        return customer is null ? null : _mapper.Map<ProfileDto>(customer);
    }

    public async Task<bool> UpdateProfileAsync(Guid customerId, UpdateProfileDto dto)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId",   customerId);
        p.Add("@Name",         dto.Name);
        p.Add("@PhoneNumber",  dto.PhoneNumber);
        await _repo.ExecuteAsync("sp_UpdateProfile", p);
        return true;
    }
}
