
using AutoMapper;
using SourceGenerator..User.Entity.Domains.User;

namespace SourceGenerator..User.Entity.Application.User.Dto.MappingConfiguration;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
    }
}