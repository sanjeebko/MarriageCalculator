using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUserIdAsync(string userId)
    {
        var user = await _userRepository.GetByUserIdAsync(userId);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new User
        {
            UserId = createUserDto.UserId,
            DisplayName = createUserDto.DisplayName,
            Email = createUserDto.Email
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToDto(createdUser);
    }

    public async Task<UserDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
    {
        var userToUpdate = new User
        {
            DisplayName = updateUserDto.DisplayName,
            Email = updateUserDto.Email
        };

        var updatedUser = await _userRepository.UpdateAsync(id, userToUpdate);
        return updatedUser != null ? MapToDto(updatedUser) : null;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<bool> UserExistsAsync(string id)
    {
        return await _userRepository.ExistsAsync(id);
    }

    public async Task<UserDto> GetOrCreateUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new System.ArgumentException("User ID claim missing from principal.");
        }

        var existingUser = await _userRepository.GetByUserIdAsync(userId);
        if (existingUser != null)
        {
            return MapToDto(existingUser);
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        var newUser = new User
        {
            UserId = userId,
            DisplayName = string.IsNullOrEmpty(name) ? userId : name,
            Email = email
        };

        var created = await _userRepository.CreateAsync(newUser);
        return MapToDto(created);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
