using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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

    #region GenerateSalt Tests

    [Fact]
    public void GenerateSalt_DefaultSize_ReturnsBase64Of16Bytes()
    {
        var result = SecurityHelper.GenerateSalt();

        Assert.Equal(16, Convert.FromBase64String(result).Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void GenerateSalt_CustomSize_ReturnsBase64OfCorrectByteLength(int size)
    {
        var result = SecurityHelper.GenerateSalt(size);

        Assert.Equal(size, Convert.FromBase64String(result).Length);
    }

    [Fact]
    public void GenerateSalt_CalledTwice_ReturnsDifferentValues()
    {
        var salt1 = SecurityHelper.GenerateSalt();
        var salt2 = SecurityHelper.GenerateSalt();

        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void GenerateSalt_ReturnValue_IsValidBase64String()
    {
        var result = SecurityHelper.GenerateSalt();

        var ex = Record.Exception(() => Convert.FromBase64String(result));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GenerateSalt_WithInvalidSize_ThrowsArgumentOutOfRangeException(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SecurityHelper.GenerateSalt(size));
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_DefaultParameters_ReturnsDeterministicHash()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash1 = SecurityHelper.HashPassword("password", salt);
        var hash2 = SecurityHelper.HashPassword("password", salt);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DefaultParameters_Returns32ByteHash()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword("password", salt);

        Assert.Equal(32, Convert.FromBase64String(hash).Length);
    }

    [Fact]
    public void HashPassword_DifferentPasswords_ReturnDifferentHashes()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash1 = SecurityHelper.HashPassword("password1", salt);
        var hash2 = SecurityHelper.HashPassword("password2", salt);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DifferentSalts_ReturnDifferentHashes()
    {
        var salt1 = SecurityHelper.GenerateSalt();
        var salt2 = SecurityHelper.GenerateSalt();
        var hash1 = SecurityHelper.HashPassword("password", salt1);
        var hash2 = SecurityHelper.HashPassword("password", salt2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_CustomNumBytesRequested_ReturnsCorrectLength()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword("password", salt, numBytesRequested: 64);

        Assert.Equal(64, Convert.FromBase64String(hash).Length);
    }

    [Fact]
    public void HashPassword_CustomPrf_ReturnsDeterministicHash()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash1 = SecurityHelper.HashPassword("password", salt, prf: KeyDerivationPrf.HMACSHA512);
        var hash2 = SecurityHelper.HashPassword("password", salt, prf: KeyDerivationPrf.HMACSHA512);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DifferentPrf_ReturnsDifferentHash()
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash256 = SecurityHelper.HashPassword("password", salt, prf: KeyDerivationPrf.HMACSHA256);
        var hash512 = SecurityHelper.HashPassword("password", salt, prf: KeyDerivationPrf.HMACSHA512);

        Assert.NotEqual(hash256, hash512);
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SecurityHelper.HashPassword(null, "abc="));
    }

    [Theory]
    [InlineData("", "abc=")]
    [InlineData("   ", "abc=")]
    public void HashPassword_EmptyOrWhitespacePassword_ThrowsArgumentException(string password, string salt)
    {
        Assert.Throws<ArgumentException>(() => SecurityHelper.HashPassword(password, salt));
    }

    [Fact]
    public void HashPassword_NullSalt_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SecurityHelper.HashPassword("password", null));
    }

    [Theory]
    [InlineData("password", "")]
    [InlineData("password", "   ")]
    public void HashPassword_EmptyOrWhitespaceSalt_ThrowsArgumentException(string password, string salt)
    {
        Assert.Throws<ArgumentException>(() => SecurityHelper.HashPassword(password, salt));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void HashPassword_InvalidIterationCount_ThrowsArgumentOutOfRangeException(int iterationCount)
    {
        var salt = SecurityHelper.GenerateSalt();
        Assert.Throws<ArgumentOutOfRangeException>(() => SecurityHelper.HashPassword("password", salt, iterationCount: iterationCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void HashPassword_InvalidNumBytesRequested_ThrowsArgumentOutOfRangeException(int numBytes)
    {
        var salt = SecurityHelper.GenerateSalt();
        Assert.Throws<ArgumentOutOfRangeException>(() => SecurityHelper.HashPassword("password", salt, numBytesRequested: numBytes));
    }

    #endregion
}
