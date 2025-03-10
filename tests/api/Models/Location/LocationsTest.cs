using System.Collections.Generic;
using Xunit;
using Scv.Api.Models.Location;
using LocationModel = Scv.Api.Models.Location.Location;

namespace tests.api.Models.Location;

public class LocationsTest
{
    [Fact]
    public void Create_ShouldReturnLocationsWithActiveLocationsOnly()
    {
        var jcLocations = new List<LocationModel>
        {
            LocationModel.Create("Location1", "1", null, true, null),
            LocationModel.Create("Location2", "2", null, false, null),
            LocationModel.Create("Location3", "3", null, true, null)
        };

        var pcssLocations = new List<LocationModel>
        {
            LocationModel.Create("Location1", "1", "1", true, null),
            LocationModel.Create("Location3", "3", "3", true, null)
        };

        var result = Locations.Create(jcLocations, pcssLocations);

        Assert.Equal(2, result.LocationList.Count);
        Assert.Contains(result.LocationList, loc => loc.Name == "Location1");
        Assert.Contains(result.LocationList, loc => loc.Name == "Location3");
    }

    [Fact]
    public void Create_ShouldReturnLocationsOrderedByName()
    {
        var jcLocations = new List<LocationModel>
        {
            LocationModel.Create("LocationB", "2", null, true, null),
            LocationModel.Create("LocationA", "1", null, true, null),
            LocationModel.Create("LocationC", "3", null, true, null)
        };

        var pcssLocations = new List<LocationModel>();

        var result = Locations.Create(jcLocations, pcssLocations);

        Assert.Equal(3, result.LocationList.Count);
        Assert.Equal("LocationA", result.LocationList[0].Name);
        Assert.Equal("LocationB", result.LocationList[1].Name);
        Assert.Equal("LocationC", result.LocationList[2].Name);
    }
}