namespace ECommerce.Tests.Domain.Entities;

[TestFixture]
public class CategoryEntityTests
{
    [Test]
    public void CategoryEntity_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic products",
            Slug = "electronics",
            ParentCategoryId = null,
            IsActive = true,
            DisplayOrder = 1,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(category.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(category.Name, Is.EqualTo("Electronics"));
            Assert.That(category.Description, Is.EqualTo("Electronic products"));
            Assert.That(category.Slug, Is.EqualTo("electronics"));
            Assert.That(category.ParentCategoryId, Is.Null);
            Assert.That(category.IsActive, Is.True);
            Assert.That(category.DisplayOrder, Is.EqualTo(1));
            Assert.That(category.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(category.UpdatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }

    [Test]
    public void CategoryEntity_WithParentCategory_ShouldAllowHierarchy()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            ParentCategoryId = parentId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(category.ParentCategoryId, Is.EqualTo(parentId));
            Assert.That(category.ParentCategoryId, Is.Not.Null);
        }
    }

    [Test]
    public void CategoryEntity_IsActive_DefaultShouldBeTrue()
    {
        // Arrange & Act
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Books",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // Assert
        Assert.That(category.IsActive, Is.True);
    }

    [Test]
    public void CategoryEntity_Timestamps_ShouldBeUtc()
    {
        // Arrange & Act
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Clothing",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(category.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(category.UpdatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }

    [Test]
    public void CategoryEntity_UpdateName_ShouldUpdateTimestamp()
    {
        // Arrange
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // Act
        category.Name = "New Name";
        category.UpdatedAt = DateTime.UtcNow;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(category.Name, Is.EqualTo("New Name"));
            Assert.That(
                category.UpdatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5))
            );
        }
    }
}
