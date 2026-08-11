using System.IO;
using ECommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Tests.Infrastructure.Services;

/// <summary>
/// Tests for DatabaseBackupService (file operations only; process invocation is not tested).
/// </summary>
[TestFixture]
public class DatabaseBackupServiceTests
{
    private string _tempDirectory;
    private Mock<ILogger<DatabaseBackupService>> _mockLogger;
    private DatabaseBackupService _service;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "backup-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);

        _mockLogger = new Mock<ILogger<DatabaseBackupService>>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=localhost;Port=5432;Database=ecommerce;Username=ecommerce;Password=ecommerce",
                    ["Backup:Directory"] = _tempDirectory,
                    ["Backup:RetentionDays"] = "7",
                }
            )
            .Build();

        _service = new DatabaseBackupService(config, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task ListBackupsAsync_ReturnsBackupFiles_NewestFirst()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectory, "20260610-100000_ecommerce.dump"), "data");
        await Task.Delay(10);
        File.WriteAllText(Path.Combine(_tempDirectory, "20260614-100000_ecommerce.dump"), "data");

        // Act
        var backups = await _service.ListBackupsAsync();

        // Assert
        backups.Should().HaveCount(2);
        backups[0].FileName.Should().Be("20260614-100000_ecommerce.dump");
        backups[0].SizeBytes.Should().Be(4);
    }

    [Test]
    public async Task ListBackupsAsync_IgnoresNonDumpFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectory, "backup.dump"), "data");
        File.WriteAllText(Path.Combine(_tempDirectory, "notes.txt"), "data");

        // Act
        var backups = await _service.ListBackupsAsync();

        // Assert
        backups.Should().HaveCount(1);
        backups[0].FileName.Should().Be("backup.dump");
    }

    [Test]
    public async Task DeleteBackupAsync_WithValidFile_DeletesIt()
    {
        // Arrange
        var fileName = "20260614-100000_ecommerce.dump";
        File.WriteAllText(Path.Combine(_tempDirectory, fileName), "data");

        // Act
        var deleted = await _service.DeleteBackupAsync(fileName);

        // Assert
        deleted.Should().BeTrue();
        File.Exists(Path.Combine(_tempDirectory, fileName)).Should().BeFalse();
    }

    [Test]
    public async Task DeleteBackupAsync_WithMissingFile_ReturnsFalse()
    {
        // Act
        var deleted = await _service.DeleteBackupAsync("does-not-exist.dump");

        // Assert
        deleted.Should().BeFalse();
    }

    [Test]
    public async Task DeleteBackupAsync_WithPathTraversal_IsRejected()
    {
        // Arrange
        var outsideFile = Path.Combine(_tempDirectory, "..", $"outside-{Guid.NewGuid():N}.txt");
        File.WriteAllText(outsideFile, "sensitive");

        // Act
        var deleted = await _service.DeleteBackupAsync("../" + Path.GetFileName(outsideFile));

        // Assert
        deleted.Should().BeFalse();
        File.Exists(outsideFile).Should().BeTrue();
        File.Delete(outsideFile);
    }

    [Test]
    public async Task RestoreAsync_WithInvalidFileName_ReturnsFalseWithoutRunningProcess()
    {
        // Act
        var restored = await _service.RestoreAsync("../../outside.dump");

        // Assert
        restored.Should().BeFalse();
    }
}
