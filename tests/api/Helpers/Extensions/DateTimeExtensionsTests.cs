using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using Scv.Api.Helpers.Extensions;
using Xunit;

namespace tests.api.Helpers.Extensions;

public class DateTimeExtensionsTests
{
    private readonly Faker _faker;

    public DateTimeExtensionsTests()
    {
        _faker = new Faker();
    }

    private Mock<HttpContext> CreateMockHttpContext(string timezoneHeaderValue = null)
    {
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var headers = new Dictionary<string, StringValues>();
        
        if (!string.IsNullOrEmpty(timezoneHeaderValue))
        {
            headers["X-Timezone"] = new StringValues(timezoneHeaderValue);
        }

        mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary(headers));
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        
        return mockHttpContext;
    }

    private IServiceProvider CreateMockServiceProvider(HttpContext httpContext = null)
    {
        var services = new ServiceCollection();
        
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        services.AddSingleton(mockHttpContextAccessor.Object);
        
        return services.BuildServiceProvider();
    }

    #region ToClientTimezone Tests

    [Fact]
    public void ToClientTimezone_WithValidTimezone_ReturnsConvertedDateTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = CreateMockHttpContext("America/New_York");

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        Assert.NotEqual(testDateTime, result);
        // The exact time will depend on DST, but we can verify it's been converted
        Assert.IsType<DateTime>(result);
    }

    [Fact]
    public void ToClientTimezone_WithNullHttpContext_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var result = testDateTime.ToClientTimezone(null);

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "Utc");
        Assert.Equal(expectedUtcTime, result);
    }

    [Fact]
    public void ToClientTimezone_WithEmptyTimezoneHeader_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = CreateMockHttpContext(string.Empty);

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "Utc");
        Assert.Equal(expectedUtcTime, result);
    }

    [Fact]
    public void ToClientTimezone_WithInvalidTimezone_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = CreateMockHttpContext("Invalid/Timezone");

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "Utc");
        Assert.Equal(expectedUtcTime, result);
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Australia/Sydney")]
    public void ToClientTimezone_WithVariousValidTimezones_ReturnsConvertedDateTime(string timezone)
    {
        // Arrange
        var testDateTime = new DateTime(2024, 6, 15, 12, 0, 0); // Summer date to test DST
        var mockHttpContext = CreateMockHttpContext(timezone);

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        Assert.IsType<DateTime>(result);
        // Verify that conversion actually happened for most timezones (unless it's the local timezone)
        var expectedConverted = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, timezone);
        Assert.Equal(expectedConverted, result);
    }

    #endregion

    #region Parameter-less Methods (Using IHttpContextAccessor)

    [Fact]
    public void ToClientTimezone_ParameterlessWithValidTimezone_ReturnsConvertedDateTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = CreateMockHttpContext("America/New_York");
        var serviceProvider = CreateMockServiceProvider(mockHttpContext.Object);
        
        DateTimeExtensions.Configure(serviceProvider);

        // Act
        var result = testDateTime.ToClientTimezone();

        // Assert
        Assert.IsType<DateTime>(result);
        var expectedNewYorkTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "America/New_York");
        Assert.Equal(expectedNewYorkTime, result);
    }

    [Fact]
    public void ToClientTimezone_ParameterlessWithNoServiceProvider_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        
        // Don't configure service provider (or configure with null)
        DateTimeExtensions.Configure(null);

        // Act
        var result = testDateTime.ToClientTimezone();

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "UTC");
        Assert.Equal(expectedUtcTime, result);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ToClientTimezone_WithHttpContextButNoRequest_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "UTC");
        Assert.Equal(expectedUtcTime, result);
    }

    [Fact]
    public void ToClientTimezone_WithWhitespaceTimezone_ReturnsDefaultUtcTime()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var mockHttpContext = CreateMockHttpContext("   ");

        // Act
        var result = testDateTime.ToClientTimezone(mockHttpContext.Object);

        // Assert
        var expectedUtcTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(testDateTime, "UTC");
        Assert.Equal(expectedUtcTime, result);
    }

    #endregion
}