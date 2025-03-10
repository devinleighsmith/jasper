using AutoMapper;
using Scv.Api.Models.Location;
using JC = JCCommon.Clients.LocationServices;
using PCSS = PCSSCommon.Models;
using System.Collections.Generic;

namespace Scv.Api.Mappers
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            // JC mappings
            CreateMap<JC.CodeValue, Location>()
                .ConstructUsing(src => Location.Create(
                    src.LongDesc,
                    src.ShortDesc,
                    src.Code,
                    src.Flex == "Y",
                    new List<CourtRoom>()))
                .ForMember(dest => dest.InfoLink, opt => opt.Ignore());

            CreateMap<JC.CodeValue, CourtRoom>()
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.Flex))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ShortDesc));

            // PCSS
            CreateMap<PCSS.CourtRoom, CourtRoom>()
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.CourtRoomCd));

            CreateMap<PCSS.Location, Location>()
                .ConstructUsing(src => Location.Create(
                    src.LocationSNm,
                    src.JustinAgenId != null ? src.JustinAgenId.ToString() : null,
                    src.LocationId != null ? src.LocationId.ToString() : null,
                    src.ActiveYn == "Y",
                    new List<CourtRoom>()))
                .ForMember(dest => dest.InfoLink, opt => opt.Ignore());
        }
    }
}
