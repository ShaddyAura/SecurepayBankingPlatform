using Application.DTOs.Auth;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings.AuthMapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // Customer entity → CustomerDto
        CreateMap<Customer, CustomerDto>();

        // RegisterDto → Customer entity
        CreateMap<RegisterDto, Customer>()
            .ForMember(dest => dest.CustomerId,      opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash,    opt => opt.Ignore())  // set in service after hashing
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.Is2FAEnabled,    opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.IsActive,        opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt,       opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<RefreshToken, RefreshTokenDto>();
    }
}
