using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for UserRepository
/// </summary>
public class UserRepositoryTests : TestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveUsers()
    {
        // Arrange
        var activeUser1 = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Active User 1",
            Email = "active1@test.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var activeUser2 = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Active User 2",
            Email = "active2@test.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Inactive User",
            Email = "inactive@test.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        await DbContext.Users.AddRangeAsync(activeUser1, activeUser2, inactiveUser);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var usersList = result.ToList();
        usersList.Should().HaveCount(2);
        usersList.Should().OnlyContain(u => u.IsActive);
        usersList.Should().Contain(u => u.Id == activeUser1.Id);
        usersList.Should().Contain(u => u.Id == activeUser2.Id);
        usersList.Should().NotContain(u => u.Id == inactiveUser.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidActiveUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Test User",
            Email = "test@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.DisplayName.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Inactive User",
            Email = "inactive@test.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        await DbContext.Users.AddAsync(inactiveUser);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(inactiveUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Test User",
            Email = email,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithCaseInsensitiveEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Test User",
            Email = email.ToLower(),
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email.ToUpper());

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email.ToLower());
    }

    [Fact]
    public async Task GetByEmailAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var email = "inactive@example.com";
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Inactive User",
            Email = email,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        await DbContext.Users.AddAsync(inactiveUser);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "New User",
            Email = "new@example.com",
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.DisplayName.Should().Be("New User");
        result.Email.Should().Be("new@example.com");

        // Verify it was saved to the database
        var savedUser = await DbContext.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.DisplayName.Should().Be("New User");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Original Name",
            Email = "original@example.com",
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(existingUser);
        await DbContext.SaveChangesAsync();

        var updateData = new User
        {
            DisplayName = "Updated Name",
            Email = "updated@example.com",
            IsEmailVerified = true
        };

        // Act
        var result = await _repository.UpdateAsync(existingUser.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingUser.Id);
        result.DisplayName.Should().Be("Updated Name");
        result.Email.Should().Be("updated@example.com");
        result.IsEmailVerified.Should().BeTrue();

        // Verify changes were persisted
        var updatedUser = await DbContext.Users.FindAsync(existingUser.Id);
        updatedUser!.DisplayName.Should().Be("Updated Name");
        updatedUser.Email.Should().Be("updated@example.com");
        updatedUser.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateData = new User
        {
            DisplayName = "Updated Name"
        };

        // Act
        var result = await _repository.UpdateAsync(nonExistentId, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "User to Delete",
            Email = "delete@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(user.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedUser = await DbContext.Users.FindAsync(user.Id);
        deletedUser.Should().NotBeNull();
        deletedUser!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingActiveUser_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Existing User",
            Email = "existing@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Inactive User",
            Email = "inactive@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        await DbContext.Users.AddAsync(inactiveUser);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(inactiveUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "existing@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Existing User",
            Email = email,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithCaseInsensitiveEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "existing@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Existing User",
            Email = email.ToLower(),
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync(email.ToUpper());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithValidUser_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = "Test User",
            Email = "test@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            LastLoginAt = null
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateLastLoginAsync(user.Id);

        // Assert
        var afterUpdate = DateTime.UtcNow;
        result.Should().NotBeNull();
        result!.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeOnOrAfter(beforeUpdate);
        result.LastLoginAt.Should().BeOnOrBefore(afterUpdate);

        // Verify changes were persisted
        var updatedUser = await DbContext.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.UpdateLastLoginAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }
}

