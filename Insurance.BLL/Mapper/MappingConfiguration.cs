using AutoMapper;
using System;
using System.Linq;

namespace Insurance.BLL.Mapper
{
    public static class MappingConfiguration
    {
        public static IMapper Init()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                var profiles = typeof(MappingConfiguration)
                .Assembly
                .GetTypes()
                .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<Profile>()
                .ToList();

                mc.AddProfiles(profiles);
            });

            var mapper = mappingConfig.CreateMapper();
            return mapper;
        }
    }
}
