using System;
using Xunit;
using LocationModel = Scv.Api.Models.Location.Location;

namespace tests.api.Models.Location;

public class LocationTest
{
    [Fact]
    public void InfoLink_ShouldReturnCorrectUri_WhenNameIsValid()
    {
        var location = new LocationModel { Name = "Vancouver Law Courts" };

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/vancouver"), result);
    }

    [Fact]
    public void InfoLink_ShouldReturnNull_WhenNameIsNull()
    {
        var location = new LocationModel { Name = null };

        var result = location.InfoLink;

        Assert.Null(result);
    }

    [Fact]
    public void InfoLink_ShouldReturnNull_WhenNameIsEmpty()
    {
        var location = new LocationModel { Name = string.Empty };

        var result = location.InfoLink;

        Assert.Null(result);
    }

    [Fact]
    public void InfoLink_ShouldFilterOutInvalidWords()
    {
        var location = new LocationModel { Name = "Court Law British Columbia Provincial Courts" };

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/british-columbia"), result);
    }

    [Fact]
    public void InfoLink_ShouldHandleMixedCaseWords()
    {
        var location = new LocationModel { Name = "BRITISH ColUmbia provincial COURT" };

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/british-columbia"), result);
    }
}
