using Microsoft.EntityFrameworkCore;
using Schmeconomics.Api.Categories;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public class CategoryServiceTests
{
    private static readonly string TEST_ACCOUNT_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_ACCOUNT_NAME = "Test Account";
    private static readonly string TEST_CATEGORY_1_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_CATEGORY_1_NAME = "Test Category 1";
    private static readonly int TEST_CATEGORY_1_BALANCE = 100;
    private static readonly int TEST_CATEGORY_1_REFILL_VALUE = 50;
    private static readonly string TEST_CATEGORY_2_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_CATEGORY_2_NAME = "Test Category 2";
    private static readonly int TEST_CATEGORY_2_BALANCE = 200;
    private static readonly int TEST_CATEGORY_2_REFILL_VALUE = 75;
    private static readonly string TEST_USER_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_USER_NAME = "Test User";

    private SchmeconomicsDbContext _dbContext = default!;
    private CategoryService _categoryService = default!;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _dbContext = await DbConfig.CreateSqliteInMemoryDbContextAsync();

        // Create test data
        var user = new User
        {
            Id = TEST_USER_ID,
            Name = TEST_USER_NAME,
            PasswordHash = "hashed_password",
            Role = Role.User
        };

        var account = new Account
        {
            Id = TEST_ACCOUNT_ID,
            Name = TEST_ACCOUNT_NAME
        };

        var category1 = new Category
        {
            Id = TEST_CATEGORY_1_ID,
            Name = TEST_CATEGORY_1_NAME,
            Balance = TEST_CATEGORY_1_BALANCE,
            RefillValue = TEST_CATEGORY_1_REFILL_VALUE,
            AccountId = TEST_ACCOUNT_ID
        };

        var category2 = new Category
        {
            Id = TEST_CATEGORY_2_ID,
            Name = TEST_CATEGORY_2_NAME,
            Balance = TEST_CATEGORY_2_BALANCE,
            RefillValue = TEST_CATEGORY_2_REFILL_VALUE,
            AccountId = TEST_ACCOUNT_ID
        };

        _dbContext.Users.Add(user);
        _dbContext.Accounts.Add(account);
        _dbContext.Categories.Add(category1);
        _dbContext.Categories.Add(category2);
        await _dbContext.SaveChangesAsync();

        _categoryService = new CategoryService(_dbContext);
    }

    [TestMethod]
    public async Task UpdateCategoriesRefillValuesAsync_WithValidAccountAndAllCategories_UpdatesRefillValues()
    {
        // Arrange
        var refillValueUpdates = new List<CategoryRefillValueUpdate>
        {
            new CategoryRefillValueUpdate(TEST_CATEGORY_1_ID, 100),
            new CategoryRefillValueUpdate(TEST_CATEGORY_2_ID, 200)
        };

        // Act
        var result = await _categoryService.UpdateCategoryRefillValuesAsync(TEST_ACCOUNT_ID, refillValueUpdates);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);

        // Verify refill values were updated in database
        var updatedCategory1 = await _dbContext.Categories.FindAsync(TEST_CATEGORY_1_ID);
        Assert.IsNotNull(updatedCategory1);
        Assert.AreEqual(100, updatedCategory1.RefillValue);

        var updatedCategory2 = await _dbContext.Categories.FindAsync(TEST_CATEGORY_2_ID);
        Assert.IsNotNull(updatedCategory2);
        Assert.AreEqual(200, updatedCategory2.RefillValue);
    }

    [TestMethod]
    public async Task UpdateCategoriesRefillValuesAsync_WithValidAccountAndSomeCategories_ReturnsMissingCategoriesError()
    {
        // Arrange
        var refillValueUpdates = new List<CategoryRefillValueUpdate>
        {
            new CategoryRefillValueUpdate(TEST_CATEGORY_1_ID, 100)
            // Missing TEST_CATEGORY_2_ID
        };

        // Act
        var result = await _categoryService.UpdateCategoryRefillValuesAsync(TEST_ACCOUNT_ID, refillValueUpdates);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<CategoryServiceError.MissingCategories>(result.Error);
    }

    [TestMethod]
    public async Task UpdateCategoriesRefillValuesAsync_WithInvalidAccountId_ReturnsAccountNotFound()
    {
        // Arrange
        var refillValueUpdates = new List<CategoryRefillValueUpdate>
        {
            new CategoryRefillValueUpdate(TEST_CATEGORY_1_ID, 100),
            new CategoryRefillValueUpdate(TEST_CATEGORY_2_ID, 200)
        };
        var invalidAccountId = "invalid-account-id";

        // Act
        var result = await _categoryService.UpdateCategoryRefillValuesAsync(invalidAccountId, refillValueUpdates);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<CategoryServiceError.AccountNotFound>(result.Error);
    }

    [TestMethod]
    public async Task UpdateCategoriesRefillValuesAsync_WithNonExistentCategoryId_IgnoresInvalidCategoryAndUpdatesValidOnes()
    {
        // Arrange
        var refillValueUpdates = new List<CategoryRefillValueUpdate>
        {
            new CategoryRefillValueUpdate(TEST_CATEGORY_1_ID, 100),
            new CategoryRefillValueUpdate("non-existent-category-id", 200)
        };

        // Act
        var result = await _categoryService.UpdateCategoryRefillValuesAsync(TEST_ACCOUNT_ID, refillValueUpdates);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<CategoryServiceError.MissingCategories>(result.Error);
    }
}
