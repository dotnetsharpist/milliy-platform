using AutoMapper;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Mappers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // User
        CreateMap<User, UserCreationDto>().ReverseMap();
        CreateMap<User, UserResultDto>().ReverseMap();
        CreateMap<User, UserUpdateDto>().ReverseMap();
    }
}