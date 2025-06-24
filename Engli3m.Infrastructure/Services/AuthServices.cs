using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Microsoft.AspNetCore.Identity;

namespace Engli3m.Infrastructure.Services
{
    public class AuthServices(UserManager<User> _userManager, RoleManager<Role> _roleManager, ITokenService _tokenService) : IAuthServices
    {



        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Email already exists"]
                };
            }

            // Create new user
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Grade = registerDto.Grade,
                PhoneNumber = registerDto.PhoneNumber
            };

            // Create user
            var createResult = await _userManager.CreateAsync(user, registerDto.Password);
            if (!createResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = createResult.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign default role
            const string defaultRole = "Student";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _roleManager.CreateAsync(new Role(defaultRole));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);
            if (!roleResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = roleResult.Errors.Select(e => e.Description).ToList()
                };
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Roles = [.. roles]
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                };
            }

            // Check password
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                };
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Roles = [.. roles]
            };
        }

        public async Task<bool> Logout(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            // Update security stamp to invalidate current token
            await _userManager.UpdateSecurityStampAsync(user);
            return true;
        }
    }
}