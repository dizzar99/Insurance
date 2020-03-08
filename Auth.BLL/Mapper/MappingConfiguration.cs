using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Mapper
{
    public static class MappingConfiguration
    {
        public static IMapper Init()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile());
            });

            var mapper = mappingConfig.CreateMapper();
            return mapper;
        }
    }
}
