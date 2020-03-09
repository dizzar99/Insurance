using AutoMapper;
using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Interface;
using Insurance.DataAccess.Models;

namespace Insurance.BLL.Mapper.Profiles
{
    class SessionProfile : Profile
    {
        public SessionProfile()
        {
            this.CreateMap<ClientHello, DbClientHello>();

            this.CreateMap<DbServerHello, ServerHello>()
                .ForMember(s => s.EllipticCurve, opt => opt.MapFrom(d => new EllipticCurve()))
                .ForMember(s => s.PublicKey, opt => opt.MapFrom((d, s) =>
                {
                    var x = d.PublicKey.X;
                    var y = d.PublicKey.Y;
                    return new ECPoint
                    {
                        X = x,
                        Y = y,
                    };
                }));

        }
    }
}
