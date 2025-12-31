namespace ECommerce.Tests.Domain.Entities;

[TestFixture]
public class CategoryEntitySimpleTests
{
    [Test]
    public void CategoryEntity_IdProperty_CanBeSet()
    {
        // Arrange
        var category = new CategoryEntity();
        var id = Guid.NewGuid();

        // Act
        category.Id = id;

        // Assert
        Assert.That(category.Id, Is.EqualTo(id));
    }

    [Test]
    public void CategoryEntity_NameProperty_CanBeSet()
    {
        // Arrange
        var category = new CategoryEntity();

        // Act
        category.Name = "Test Category";

        // Assert
        Assert.That(category.Name, Is.EqualTo("Test Category"));
    }

    [Test]
    public void CategoryEntity_SlugProperty_CanBeSet()
    {
        // Arrange
        var category = new CategoryEntity();

        // Act
        category.Slug = "test-category";

        // Assert
        Assert.That(category.Slug, Is.EqualTo("test-category"));
    }

    [Test]
    public void CategoryEntity_DescriptionProperty_CanBeSet()
    {
        // Arrange
        var category = new CategoryEntity();

        // Act
        category.Description = "This is a test category";

        // Assert
        Assert.That(category.Description, Is.EqualTo("This is a test category"));
    }

    [Test]
    public void CategoryEntity_DisplayOrderProperty_CanBeSet()
    {
        // Arrange
        var category = new CategoryEntity();

        // Act
        category.DisplayOrder = 10;

        // Assert
        Assert.That(category.DisplayOrder, Is.EqualTo(10));
    }
}
