using Application.DTOs.Account;
using Application.DTOs.Profile;
using Application.DTOs.Transaction;
using Application.DTOs.Transfer;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings.DashboardMapping;

public class DashboardMappingProfile : Profile
{
    public DashboardMappingProfile()
    {
        CreateMap<Account,     AccountDto>();
        CreateMap<Transaction, TransactionDto>();
        CreateMap<Transaction, TransferResultDto>();
        CreateMap<Customer,    ProfileDto>();
    }
}
