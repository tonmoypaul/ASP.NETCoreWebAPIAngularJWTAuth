using ASP.NETCoreWebAPIAngularJWTAuth.Dtos;
using ASP.NETCoreWebAPIAngularJWTAuth.Models;
using AutoMapper;

namespace ASP.NETCoreWebAPIAngularJWTAuth.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserDto>();

            // we don't want to set isActive value from client request
            CreateMap<ApplicationUserDto, ApplicationUser>()
                .ForMember(dto => dto.IsActive, map => map.Ignore());
        }
        
    }
}