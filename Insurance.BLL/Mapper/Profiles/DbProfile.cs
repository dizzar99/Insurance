using AutoMapper;
using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Microsoft.AspNetCore.Identity;

namespace Insurance.BLL.Mapper.Profiles
{
    class DbProfile : Profile
    {
        public DbProfile()
        {
            this.CreateMap<RegisterUserRequest, IdentityUser>();
            this.CreateMap<LoginUserRequest, IdentityUser>();
        }
    }
}
