using System.Text;
using Comex.Application.Services;

namespace Comex.Tests.Application.Services;

/// <summary>
/// Tests for JwtSecretGenerator
/// </summary>
[TestFixture]
public class JwtSecretGeneratorTests
{
    [Test]
    public void GenerateSecret_ReturnsBase64String()
    {
        // Act
        var secret = JwtSecretGenerator.GenerateSecret();

        // Assert
        secret.Should().NotBeNullOrWhiteSpace();
        var decoded = Convert.FromBase64String(secret);
        decoded.Should().HaveCount(JwtSecretGenerator.DefaultByteCount);
    }

    [Test]
    public void GenerateSecret_ProducesUniqueValues()
    {
        // Act
        var first = JwtSecretGenerator.GenerateSecret();
        var second = JwtSecretGenerator.GenerateSecret();

        // Assert
        first.Should().NotBe(second);
    }

    [Test]
    public void GenerateSecret_WithCustomByteCount_ReturnsRequestedLength()
    {
        // Act
        var secret = JwtSecretGenerator.GenerateSecret(64);

        // Assert
        Convert.FromBase64String(secret).Should().HaveCount(64);
    }

    [Test]
    public void GenerateSecret_WithTooFewBytes_Throws()
    {
        // Act
        Action act = () => JwtSecretGenerator.GenerateSecret(16);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ValidateSecret_WithNull_ReturnsInvalid()
    {
        // Act
        var result = JwtSecretGenerator.ValidateSecret(null, isDevelopment: false);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ValidateSecret_WithShortSecret_ReturnsInvalid()
    {
        // Arrange
        var shortSecret = "too-short";

        // Act
        var result = JwtSecretGenerator.ValidateSecret(shortSecret, isDevelopment: false);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("32 bytes");
    }

    [Test]
    public void ValidateSecret_WithGeneratedSecret_IsValid()
    {
        // Arrange
        var secret = JwtSecretGenerator.GenerateSecret();

        // Act
        var result = JwtSecretGenerator.ValidateSecret(secret, isDevelopment: false);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Test]
    public void ValidateSecret_WithStrongManualSecret_IsValid()
    {
        // Arrange
        var secret = "aB3!x9K#mQ2$zW5^pR7&tV1@nJ4%cF6*";

        // Act
        var result = JwtSecretGenerator.ValidateSecret(secret, isDevelopment: false);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void ValidateSecret_WithWeakButLongSecret_RejectedInProduction()
    {
        // Arrange - lowercase only, but long enough
        var secret = new string('a', 64);

        // Act
        var result = JwtSecretGenerator.ValidateSecret(secret, isDevelopment: false);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("character classes");
    }

    [Test]
    public void ValidateSecret_WithWeakButLongSecret_WarnsInDevelopment()
    {
        // Arrange
        var secret = new string('a', 64);

        // Act
        var result = JwtSecretGenerator.ValidateSecret(secret, isDevelopment: true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warning.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ValidateSecret_ByteLength_NotCharacterCount()
    {
        // Arrange - multibyte characters, length check must use byte count
        var secret = new string('á', 32);

        // Act
        var result = JwtSecretGenerator.ValidateSecret(secret, isDevelopment: false);

        // Assert - 32 characters of 2 bytes each = 64 bytes, so length passes;
        // but it's all the same class so it must fail on entropy
        Encoding.UTF8.GetByteCount(secret).Should().Be(64);
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("character classes");
    }
}
