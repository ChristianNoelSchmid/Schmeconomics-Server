using Microsoft.EntityFrameworkCore;
using Schmeconomics.Api.Accounts;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public class AccountServiceTests
{
    private static readonly string TEST_ACCOUNT_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_ACCOUNT_NAME = "Test Account";
    private static readonly string TEST_USER_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_USER_NAME = "Test User";
    private static readonly string TEST_ACCOUNT_ID_2 = Guid.NewGuid().ToString();
    private static readonly string TEST_ACCOUNT_NAME_2 = "Test Account 2";

    private SchmeconomicsDbContext _dbContext = default!;
    private AccountService _accountService = default!;

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

        var account2 = new Account
        {
            Id = TEST_ACCOUNT_ID_2,
            Name = TEST_ACCOUNT_NAME_2
        };

        _dbContext.Users.Add(user);
        _dbContext.Accounts.Add(account);
        _dbContext.Accounts.Add(account2);
        await _dbContext.SaveChangesAsync();

        _accountService = new AccountService(_dbContext);
    }

    [TestMethod]
    public async Task GetAccountAsync_WithValidId_ReturnsAccount()
    {
        // Act
        var result = await _accountService.GetAccountAsync(TEST_ACCOUNT_ID);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(TEST_ACCOUNT_ID, result.Value.Id);
        Assert.AreEqual(TEST_ACCOUNT_NAME, result.Value.Name);
    }

    [TestMethod]
    public async Task GetAccountAsync_WithInvalidId_ReturnsAccountNotFound()
    {
        // Arrange
        var invalidAccountId = "invalid-id";

        // Act
        var result = await _accountService.GetAccountAsync(invalidAccountId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountNotFound>(result.Error);
        Assert.AreEqual($"Account with id '{invalidAccountId}' not found", result.Error.Message);
    }

    [TestMethod]
    public async Task GetAllAccountsAsync_ReturnsAllAccounts()
    {
        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(2, result.Value.Count());
        Assert.IsTrue(result.Value.Any(a => a.Id == TEST_ACCOUNT_ID));
        Assert.IsTrue(result.Value.Any(a => a.Id == TEST_ACCOUNT_ID_2));
    }

    [TestMethod]
    public async Task CreateAccountAsync_WithUniqueName_CreatesAccount()
    {
        // Arrange
        var newAccountName = "New Test Account";

        // Act
        var result = await _accountService.CreateAccountAsync(newAccountName);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(newAccountName, result.Value.Name);
        
        // Verify account was actually saved to database
        var createdAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Name == newAccountName);
        Assert.IsNotNull(createdAccount);
        Assert.AreEqual(newAccountName, createdAccount.Name);
    }

    [TestMethod]
    public async Task CreateAccountAsync_WithDuplicateName_ReturnsAccountAlreadyExists()
    {
        // Act
        var result = await _accountService.CreateAccountAsync(TEST_ACCOUNT_NAME);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountAlreadyExists>(result.Error);
        Assert.AreEqual($"Account with name '{TEST_ACCOUNT_NAME}' already exists", result.Error.Message);
    }

    [TestMethod]
    public async Task ToggleUserToAccountAsync_WithValidAccountAndUser_AddsAssociation()
    {
        // Act
        var result = await _accountService.ToggleUserToAccountAsync(TEST_ACCOUNT_ID, TEST_USER_ID);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        
        // Verify association was created
        var accountUser = await _dbContext.AccountUsers
            .FirstOrDefaultAsync(au => au.AccountId == TEST_ACCOUNT_ID && au.UserId == TEST_USER_ID);
        Assert.IsNotNull(accountUser);
    }

    [TestMethod]
    public async Task ToggleUserToAccountAsync_WithValidAccountAndUser_WhenAssociationExists_RemovesIt()
    {
        // First add the association
        await _accountService.ToggleUserToAccountAsync(TEST_ACCOUNT_ID, TEST_USER_ID);

        // Act - toggle again to remove the association
        var result = await _accountService.ToggleUserToAccountAsync(TEST_ACCOUNT_ID, TEST_USER_ID);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        
        // Verify association was removed
        var accountUser = await _dbContext.AccountUsers
            .FirstOrDefaultAsync(au => au.AccountId == TEST_ACCOUNT_ID && au.UserId == TEST_USER_ID);
        Assert.IsNull(accountUser);
    }

    [TestMethod]
    public async Task ToggleUserToAccountAsync_WithNonExistentAccount_ReturnsAccountNotFound()
    {
        // Arrange
        var nonExistentAccountId = "non-existent-account-id";

        // Act
        var result = await _accountService.ToggleUserToAccountAsync(nonExistentAccountId, TEST_USER_ID);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountNotFound>(result.Error);
        Assert.AreEqual($"Account with id '{nonExistentAccountId}' not found", result.Error.Message);
    }

    [TestMethod]
    public async Task ToggleUserToAccountAsync_WithNonExistentUser_ReturnsUserNotFound()
    {
        // Arrange
        var nonExistentUserId = "non-existent-user-id";

        // Act
        var result = await _accountService.ToggleUserToAccountAsync(TEST_ACCOUNT_ID, nonExistentUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.UserNotFound>(result.Error);
        Assert.AreEqual($"User with id '{nonExistentUserId}' not found", result.Error.Message);
    }

    [TestMethod]
    public async Task DeleteAccountAsync_WithValidId_DeletesAccount()
    {
        // Act
        var result = await _accountService.DeleteAccountAsync(TEST_ACCOUNT_ID);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        
        // Verify account was actually deleted from database
        var deletedAccount = await _dbContext.Accounts.FindAsync(TEST_ACCOUNT_ID);
        Assert.IsNull(deletedAccount);
    }

    [TestMethod]
    public async Task DeleteAccountAsync_WithInvalidId_ReturnsAccountNotFound()
    {
        // Arrange
        var invalidAccountId = "invalid-id";

        // Act
        var result = await _accountService.DeleteAccountAsync(invalidAccountId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountNotFound>(result.Error);
        Assert.AreEqual($"Account with id '{invalidAccountId}' not found", result.Error.Message);
    }

    [TestMethod]
    public async Task UpdateAccountAsync_WithValidIdAndName_UpdatesAccount()
    {
        // Arrange
        var newName = "Updated Account Name";

        // Act
        var result = await _accountService.UpdateAccountAsync(TEST_ACCOUNT_ID, newName);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(newName, result.Value.Name);
        
        // Verify account was actually updated in database
        var updatedAccount = await _dbContext.Accounts.FindAsync(TEST_ACCOUNT_ID);
        Assert.IsNotNull(updatedAccount);
        Assert.AreEqual(newName, updatedAccount.Name);
    }

    [TestMethod]
    public async Task UpdateAccountAsync_WithValidIdAndNullName_DoesNotUpdate()
    {
        // Act
        var result = await _accountService.UpdateAccountAsync(TEST_ACCOUNT_ID, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(TEST_ACCOUNT_NAME, result.Value.Name);
        
        // Verify account was not updated in database
        var updatedAccount = await _dbContext.Accounts.FindAsync(TEST_ACCOUNT_ID);
        Assert.IsNotNull(updatedAccount);
        Assert.AreEqual(TEST_ACCOUNT_NAME, updatedAccount.Name);
    }

    [TestMethod]
    public async Task UpdateAccountAsync_WithValidIdAndDuplicateName_ReturnsAccountAlreadyExists()
    {
        // Act
        var result = await _accountService.UpdateAccountAsync(TEST_ACCOUNT_ID, TEST_ACCOUNT_NAME_2);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountAlreadyExists>(result.Error);
        Assert.AreEqual($"Account with name '{TEST_ACCOUNT_NAME_2}' already exists", result.Error.Message);
    }

    [TestMethod]
    public async Task UpdateAccountAsync_WithInvalidId_ReturnsAccountNotFound()
    {
        // Arrange
        var invalidAccountId = "invalid-id";

        // Act
        var result = await _accountService.UpdateAccountAsync(invalidAccountId, "New Name");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsError);
        Assert.IsInstanceOfType<AccountServiceError.AccountNotFound>(result.Error);
        Assert.AreEqual($"Account with id '{invalidAccountId}' not found", result.Error.Message);
    }
}
