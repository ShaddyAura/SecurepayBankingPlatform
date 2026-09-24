using Application.DTOs.Profile;

namespace Application.Interfaces.IProfileService;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid customerId);
    Task<bool> UpdateProfileAsync(Guid customerId, UpdateProfileDto dto);
}
