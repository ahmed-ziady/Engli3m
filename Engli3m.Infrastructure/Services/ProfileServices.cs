using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Post;
using Engli3m.Application.DTOs.Profile;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace Engli3m.Infrastructure.Services
{
    public class ProfileService : IProfile
    {
        private readonly EnglishDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly string _uploadFolder;

        public ProfileService(
            EnglishDbContext dbContext,
            UserManager<User> userManager)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Profile", "images");
            Directory.CreateDirectory(_uploadFolder);
        }

        public async Task<List<FavPostDto>> GetFavPostAsync(int userId)
        {
            var posts = await _dbContext.FavPosts
      .Where(f => f.UserId == userId)
      .Select(f => new FavPostDto
      {
          PostId            = f.Post.Id,
          FirstName         = f.Post.User.FirstName,
          LastName          = f.Post.User.LastName,
          ProfilePictureUrl = f.Post.User.ProfilePictureUrl ?? string.Empty,
          Content           = f.Post.Content,
          CreatedAt         = f.Post.CreatedAt,
          MediaUrls         = f.Post.Media.Select(m => m.Url).ToList()
      }).OrderByDescending(p => p.CreatedAt)
      .ToListAsync();


            return posts;
        }

        public async Task<IEnumerable<GetLectureProgressDto>> GetSubmittedProgressAsync(int studentId)
        {
            return await _dbContext.VideoProgress
                .Where(vp => vp.StudentId == studentId)
                .Select(vp => new GetLectureProgressDto
                {
                    LectureTitle = vp.Video.Title, // Assuming: Video → Lecture → Title
                    IsWached = vp.IsWatched,
                    Seconds = vp.WatchedSeconds
                }).OrderByDescending(p => p.LectureTitle)
                .ToListAsync();
        }


        public async Task<ProfileResponseDto> GetProfileAsync(int userId)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new ProfileResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Grade = u.Grade,
                    ProfilePictureUrl = u.ProfilePictureUrl
                })
                .FirstOrDefaultAsync()??throw new InvalidOperationException("Profile not found.");
            return user;
        }

        public async Task<string> ResetPasswordAsync(int userId, string newPassword)
        {
            // Validate input
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(newPassword));

            if (newPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(newPassword));

            var user = await _userManager.FindByIdAsync(userId.ToString())
                       ?? throw new KeyNotFoundException("User not found.");

            
                user.CurrentJwtToken = null;
                await _userManager.UpdateAsync(user);
           

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset password using Identity’s hashing
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Password reset failed: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
            }

            return "Password reset successfully.";
        }

        public async Task<bool> UpdatePhoneNumber(int userId, string phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            var setResult = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
            return setResult.Succeeded;
        }

        public async Task<bool> UpdateProfilePicture(int userId, ProfileImageDto imageDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            string? savedFileName = null;

            try
            {
                // Remove old image
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    var oldFile = Path.Combine(_uploadFolder, Path.GetFileName(user.ProfilePictureUrl));
                    if (File.Exists(oldFile)) File.Delete(oldFile);
                }

                savedFileName = await FileHelper.SaveImageAsync(imageDto.ProfileImage, _uploadFolder);
                if (string.IsNullOrEmpty(savedFileName))
                    throw new InvalidOperationException("لم يتم حفظ الصورة بالرجاء اعادة المحاولة");

                user.ProfilePictureUrl = $"/uploads/Profile/images/{savedFileName}";
                await _userManager.UpdateAsync(user);
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();

                // Clean up partial file
                if (!string.IsNullOrEmpty(savedFileName))
                {
                    var partial = Path.Combine(_uploadFolder, savedFileName);
                    if (File.Exists(partial)) File.Delete(partial);
                }

                return false;
            }
        }



    }
}
