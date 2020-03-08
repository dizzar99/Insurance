using Auth.BLL.Interface.Models.SessionModels;
using Auth.Common.Interface;
using Auth.DataAccess.Models;
using AutoMapper;
using System;

namespace Auth.BLL.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //this.CreateMap<DbRsaKey, RsaPublicKey>()
            //    .BeforeMap((dbRsaKey, rsaKey) =>
            //    {
            //        if (dbRsaKey.ExpiredDate < DateTime.Now)
            //        {
            //            throw new ServerKeyExpiredException();
            //        }
            //    });

            this.CreateMap<Session, DbSession>()
                .ForMember(d => d.Created, opt => opt.MapFrom(p => DateTime.UtcNow))
                .ReverseMap();

            //this.CreateMap<DbSymmetricKey, SymmetricKey>().ReverseMap();

            //this.CreateMap<DbRsaKey, RsaKey>();

            this.CreateMap<ClientHello, DbClientHello>();

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
