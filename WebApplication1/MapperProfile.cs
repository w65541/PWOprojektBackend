using AutoMapper;
using Database.Dto;
using Database.Entities;

namespace WebApplication1
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserDto>().ForMember(dest => dest.Type, act => act.MapFrom(src => src.Type.Name));
            CreateMap<UserDto, User>().ForMember(dest => dest.Type, act => act.MapFrom(src => new UserType {Id=src.TypeId,Name=src.Type }));
        }
    }
}
