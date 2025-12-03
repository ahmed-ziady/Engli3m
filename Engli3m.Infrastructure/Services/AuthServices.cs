using Engli3m.Application.DTOs.Auth;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure.Services
{
    public class AuthServices(UserManager<User> _userManager, RoleManager<Role> _roleManager, ITokenService _tokenService, EnglishDbContext _db) : IAuthServices
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // التحقق من وجود البريد الإلكتروني مسبقًا
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["البريد الإلكتروني موجود بالفعل"]
                };
            }

            // إنشاء مستخدم جديد
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Grade = registerDto.Grade,
                PhoneNumber = registerDto.PhoneNumber,
                //CleanPassword = registerDto.Password,
                IsLocked = true
            };

            // إنشاء المستخدم
            var createResult = await _userManager.CreateAsync(user, registerDto.Password);
            if (!createResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = createResult.Errors.Select(e => e.Description).ToList()
                };
            }

            // تعيين الدور الافتراضي
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

            // إنشاء توكن JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                GradeLevel=user.Grade.Value,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Roles = [.. roles]
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // البحث عن المستخدم عن طريق البريد الإلكتروني
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["البريد الإلكتروني أو كلمة المرور غير صحيحة"]
                };
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["البريد الإلكتروني أو كلمة المرور غير صحيحة"]
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (user.IsLocked && roles.Contains("Student"))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["تم قفل حسابك. يرجى التواصل مع المعلم أو المساعد لاعادة فتح الحساب."]
                };
            }
            var phoneNumber = user.PhoneNumber;
            var existPhone = await _userManager.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != user.Id);
            if (existPhone)
            {
                user.CurrentJwtToken =null;
            }
            if (!string.IsNullOrWhiteSpace(user.CurrentJwtToken) && user.TokenExpiry > DateTime.UtcNow)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = ["أنت بالفعل مسجل دخول!"]
                };
            }
            var token = _tokenService.GenerateJwtToken(user, roles);


            user.CurrentJwtToken = token;

            user.TokenExpiry = DateTime.UtcNow.AddYears(4);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                GradeLevel = roles.Contains("Student") && user.Grade.HasValue ? user.Grade.Value : null,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Roles = [.. roles]
            };
        }

        public async Task<bool> Logout(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            await _userManager.UpdateSecurityStampAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _db.Users
                .Include(u => u.EQuizzes)
                    .ThenInclude(eq => eq.Questions)
                .Include(u => u.EQuizSubmissions)
                    .ThenInclude(s => s.EQuestionSubmissions)
                .Include(u => u.QuizSubmissions)
                .Include(u => u.FavPosts)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            foreach (var submission in user.EQuizSubmissions.ToList())
            {
                _db.EQuestionSubmissions.RemoveRange(submission.EQuestionSubmissions);
                _db.EQuizSubmissions.Remove(submission);
            }

            _db.QuizSubmissions.RemoveRange(user.QuizSubmissions);

            foreach (var equiz in user.EQuizzes.ToList())
            {
                _db.QuestionsAnswers.RemoveRange(equiz.Questions.SelectMany(q => q.Answers));
                _db.QuizQuestions.RemoveRange(equiz.Questions);
                _db.EQuizzes.Remove(equiz);
            }

            foreach (var post in user.Posts.ToList())
            {
                _db.FavPosts.RemoveRange(post.FavPosts);
                _db.Posts.Remove(post);
            }

            _db.FavPosts.RemoveRange(user.FavPosts);

            _db.Users.Remove(user);

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
