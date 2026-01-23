namespace Edi.AspNetCore.Utils.Tests;

public class SecurityHelperTests
{
    #region SterilizeLink Tests

    [Theory]
    [InlineData("https://www.example.com", "https://www.example.com")]
    [InlineData("http://www.example.com", "http://www.example.com")]
    [InlineData("https://subdomain.example.com/path/to/resource", "https://subdomain.example.com/path/to/resource")]
    [InlineData("https://example.com:443/page?query=value", "https://example.com:443/page?query=value")]
    public void SterilizeLink_WithValidUrls_ReturnsOriginalUrl(string rawUrl, string expected)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void SterilizeLink_WithNullOrWhitespace_ReturnsHash(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal("#", result);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/home")]
    [InlineData("/path/to/page")]
    [InlineData("/page?query=value")]
    [InlineData("/page#anchor")]
    public void SterilizeLink_WithValidLocalSlash_ReturnsOriginalUrl(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal(rawUrl, result);
    }

    [Theory]
    [InlineData("//")]
    [InlineData("//example.com")]
    [InlineData("/\\")]
    [InlineData("/\\example.com")]
    public void SterilizeLink_WithDoubleSlashOrBackslash_ReturnsHash(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal("#", result);
    }

    [Theory]
    [InlineData("http://localhost")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://localhost")]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://127.0.0.1:3000")]
    [InlineData("https://127.0.0.1")]
    public void SterilizeLink_WithLoopbackAddress_ReturnsHash(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal("#", result);
    }

    [Theory]
    [InlineData("http://192.168.1.1")]
    [InlineData("http://192.168.0.100")]
    [InlineData("http://192.168.255.255")]
    [InlineData("http://10.0.0.1")]
    [InlineData("http://10.255.255.255")]
    [InlineData("http://172.16.0.1")]
    [InlineData("http://172.31.255.255")]
    [InlineData("http://172.20.0.1")]
    public void SterilizeLink_WithPrivateIPv4Address_ReturnsHash(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal("#", result);
    }

    [Theory]
    [InlineData("http://8.8.8.8")]
    [InlineData("http://1.1.1.1")]
    [InlineData("http://93.184.216.34")]
    [InlineData("http://172.15.0.1")]
    [InlineData("http://172.32.0.1")]
    [InlineData("http://191.168.1.1")]
    [InlineData("http://193.168.1.1")]
    public void SterilizeLink_WithPublicIPv4Address_ReturnsOriginalUrl(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal(rawUrl, result);
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("not a url")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("ftp://example.com")]
    public void SterilizeLink_WithInvalidUrl_ReturnsHash(string rawUrl)
    {
        // Act
        var result = SecurityHelper.SterilizeLink(rawUrl);

        // Assert
        Assert.Equal("#", result);
    }

    #endregion

    #region IsPrivateIP Tests

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("192.168.0.1", true)]
    [InlineData("192.168.255.255", true)]
    [InlineData("192.168.0.0", true)]
    public void IsPrivateIP_With192_168Range_ReturnsTrue(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("10.0.0.0", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("10.255.255.255", true)]
    [InlineData("10.10.10.10", true)]
    public void IsPrivateIP_With10Range_ReturnsTrue(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.0.0.0", true)]
    [InlineData("127.255.255.255", true)]
    [InlineData("127.1.1.1", true)]
    public void IsPrivateIP_With127Range_ReturnsTrue(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("172.16.0.0", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("172.31.255.255", true)]
    [InlineData("172.20.10.5", true)]
    [InlineData("172.25.100.200", true)]
    public void IsPrivateIP_With172_16To31Range_ReturnsTrue(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("8.8.8.8", false)]
    [InlineData("1.1.1.1", false)]
    [InlineData("93.184.216.34", false)]
    [InlineData("142.250.180.46", false)]
    [InlineData("151.101.1.140", false)]
    [InlineData("172.15.255.255", false)]
    [InlineData("172.32.0.0", false)]
    [InlineData("191.168.1.1", false)]
    [InlineData("193.168.1.1", false)]
    public void IsPrivateIP_WithPublicIP_ReturnsFalse(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("172.15.0.1", false)]
    [InlineData("172.32.0.1", false)]
    public void IsPrivateIP_WithEdgeCasesOutsidePrivateRange_ReturnsFalse(string ip, bool expected)
    {
        // Act
        var result = SecurityHelper.IsPrivateIP(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("")]
    [InlineData("256.256.256.256")]
    public void IsPrivateIP_WithInvalidIP_ThrowsFormatException(string ip)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SecurityHelper.IsPrivateIP(ip));
    }

    [Fact]
    public void IsPrivateIP_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string ip = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SecurityHelper.IsPrivateIP(ip));
    }

    #endregion
}
