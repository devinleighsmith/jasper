using System;
using Xunit;
using LocationModel = Scv.Api.Models.Location.Location;

namespace tests.api.Models.Location;

public class LocationTest
{
    [Fact]
    public void InfoLink_ShouldReturnCorrectUri_WhenNameIsValid()
    {
        var location = LocationModel.Create("Vancouver Law Courts", default, default, default, default);

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/vancouver"), result);
    }

    [Fact]
    public void InfoLink_ShouldReturnNull_WhenNameIsNull()
    {
        var location = LocationModel.Create(null, default, default, default, default);

        var result = location.InfoLink;

        Assert.Null(result);
    }

    [Fact]
    public void InfoLink_ShouldReturnNull_WhenNameIsEmpty()
    {
        var location = LocationModel.Create(string.Empty, default, default, default, default);

        var result = location.InfoLink;

        Assert.Null(result);
    }

    [Fact]
    public void InfoLink_ShouldFilterOutInvalidWords()
    {
        var location = LocationModel.Create("Court Law British Columbia Provincial Courts", default, default, default, default);

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/british-columbia"), result);
    }

    [Fact]
    public void InfoLink_ShouldHandleMixedCaseWords()
    {
        var location = LocationModel.Create("BRITISH ColUmbia provincial COURT", default, default, default, default);

        var result = location.InfoLink;

        Assert.Equal(new Uri("https://provincialcourt.bc.ca/court-locations/british-columbia"), result);
    }
}
