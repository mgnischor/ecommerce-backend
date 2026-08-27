using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Comex.Infrastructure.Services;

namespace Comex.Tests.Infrastructure.Services;

/// <summary>
/// Tests for CertificateGenerator
/// </summary>
[TestFixture]
public class CertificateGeneratorTests
{
    private string _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public void CreateSelfSignedCertificate_CreatesPfxFile()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "test.pfx");

        // Act
        var cert = CertificateGenerator.CreateSelfSignedCertificate(
            outputPath,
            password: "test-password",
            host: "localhost",
            daysValid: 30
        );

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        cert.Subject.Should().Contain("localhost");
        cert.NotAfter.Should().BeAfter(DateTime.Now);
        cert.HasPrivateKey.Should().BeTrue();
        cert.Dispose();
    }

    [Test]
    public void CreateSelfSignedCertificate_WithInvalidDays_Throws()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "invalid.pfx");

        // Act
        Action act = () =>
            CertificateGenerator.CreateSelfSignedCertificate(
                outputPath,
                password: null,
                host: "localhost",
                daysValid: 0
            );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void LoadOrCreate_WhenFileExists_LoadsExisting()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "existing.pfx");
        using var original = CertificateGenerator.CreateSelfSignedCertificate(
            outputPath,
            password: "pwd",
            host: "localhost",
            daysValid: 30
        );
        var originalThumbprint = original.Thumbprint;

        // Act
        using var loaded = CertificateGenerator.LoadOrCreate(
            outputPath,
            password: "pwd",
            host: "localhost",
            daysValid: 30
        );

        // Assert
        loaded.Thumbprint.Should().Be(originalThumbprint);
    }

    [Test]
    public void LoadOrCreate_WhenFileMissing_CreatesNew()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "new.pfx");

        // Act
        using var cert = CertificateGenerator.LoadOrCreate(
            outputPath,
            password: "pwd",
            host: "localhost",
            daysValid: 30
        );

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        cert.HasPrivateKey.Should().BeTrue();
    }

    [Test]
    public void GeneratedCertificate_HasServerAuthEku()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "eku.pfx");

        // Act
        using var cert = CertificateGenerator.CreateSelfSignedCertificate(
            outputPath,
            password: null,
            host: "localhost",
            daysValid: 30
        );

        // Assert
        var eku = cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        eku.Should().NotBeNull();
        var ekuValues = eku!.EnhancedKeyUsages.Cast<Oid>().Select(o => o.Value);
        ekuValues.Should().Contain("1.3.6.1.5.5.7.3.1");
    }
}
