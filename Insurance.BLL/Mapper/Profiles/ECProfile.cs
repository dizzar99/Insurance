using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Interface;
using Insurance.DataAccess.Models;
using AutoMapper;

namespace Insurance.BLL.Mapper.Profiles
{
    class ECProfile : Profile
    {
        public ECProfile()
        {
            this.CreateMap<ECPoint, DbECPoint>().ReverseMap();

            this.CreateMap<EllipticCurve, ECParameters>()
                .ForMember(p => p.A, opt => opt.MapFrom(i => i.A.ToByteArray()))
                .ForMember(p => p.B, opt => opt.MapFrom(i => i.B.ToByteArray()))
                .ForMember(p => p.P, opt => opt.MapFrom(i => i.P.ToByteArray()))
                .ForMember(p => p.N, opt => opt.MapFrom(i => i.N.ToByteArray()))
                .ForMember(p => p.G, opt => opt.MapFrom((i, d) =>
                {
                    var x = i.G.X.ToByteArray();
                    var y = i.G.Y.ToByteArray();
                    return new ECPoint
                    {
                        X = x,
                        Y = y,
                    };
                }));
        }
    }
}
