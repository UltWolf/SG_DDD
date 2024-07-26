
using AutoMapper;
using SourceGenerator.Domain.User.Entity;  
using SourceGenerator.Application.User.Dto;
namespace SourceGenerator.Application.User.Dto.MappingConfiguration;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UserEntity, UserDto>();
        CreateMap<CreateUserDto, UserEntity>();
    }
}