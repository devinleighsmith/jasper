using AutoMapper;
using Scv.Api.Mappers;
using Xunit;
using JC = JCCommon.Clients.LocationServices;
using PCSS = PCSSCommon.Models;
using Location = Scv.Api.Models.Location.Location;
using CourtRoom = Scv.Api.Models.Location.CourtRoom;

namespace tests.api.Mappers;

public class LocationProfileTests
{
    private readonly IMapper _mapper;

    public LocationProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<LocationProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void JC_CodeValue_To_Location_Mapping_ShouldBeValid()
    {
        var source = new JC.CodeValue
        {
            LongDesc = "Long Description",
            ShortDesc = "Short Description",
            Code = "123",
            Flex = "Y"
        };

        var result = _mapper.Map<Location>(source);

        Assert.Equal("Long Description", result.Name);
        Assert.Equal("Short Description", result.Code);
        Assert.Equal("123", result.LocationId);
        Assert.True(result.Active);
        Assert.Empty(result.CourtRooms);
    }

    [Fact]
    public void JC_CodeValue_To_CourtRoom_Mapping_ShouldBeValid()
    {
        var source = new JC.CodeValue
        {
            Flex = "LocationId",
            Code = "RoomCode",
            ShortDesc = "RoomType"
        };

        var result = _mapper.Map<CourtRoom>(source);

        Assert.Equal("LocationId", result.LocationId);
        Assert.Equal("RoomCode", result.Room);
        Assert.Equal("RoomType", result.Type);
    }

    [Fact]
    public void PCSS_CourtRoom_To_CourtRoom_Mapping_ShouldBeValid()
    {
        var source = new PCSS.CourtRoom
        {
            CourtRoomCd = "RoomCode"
        };

        var result = _mapper.Map<CourtRoom>(source);

        Assert.Equal("RoomCode", result.Room);
    }

    [Fact]
    public void PCSS_Location_To_Location_Mapping_ShouldBeValid()
    {
        var source = new PCSS.Location
        {
            LocationSNm = "Location Name",
            JustinAgenId = 123,
            LocationId = 456,
            ActiveYn = "Y"
        };

        var result = _mapper.Map<Location>(source);

        Assert.Equal("Location Name", result.Name);
        Assert.Equal("123", result.Code);
        Assert.Equal("456", result.LocationId);
        Assert.True(result.Active);
        Assert.Empty(result.CourtRooms);
    }

    [Fact]
    public void PCSS_Location_To_Location_Mapping_ShouldHandleNullValues()
    {
        var source = new PCSS.Location
        {
            LocationSNm = "Location Name",
            JustinAgenId = null,
            LocationId = null,
            ActiveYn = "N"
        };

        var result = _mapper.Map<Location>(source);

        Assert.Equal("Location Name", result.Name);
        Assert.Null(result.Code);
        Assert.Null(result.LocationId);
        Assert.False(result.Active);
        Assert.Empty(result.CourtRooms);
    }
}