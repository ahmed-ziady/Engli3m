using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
namespace Engli3m.Infrastructure.Services
{
    public class AdminServices : IAdminService
    {
        private readonly EnglishDbContext _db;
        private readonly string _lectureFolder;
        private readonly string _quizFolder;

        public AdminServices(EnglishDbContext db)
        {
            _db = db;
            _lectureFolder = Path.Combine("wwwroot", "uploads", "lectures", "videos");
            _quizFolder = Path.Combine("wwwroot", "uploads", "quizzes", "images");
            Directory.CreateDirectory(_lectureFolder);
            Directory.CreateDirectory(_quizFolder);
        }

        public async Task<Int32> UploadLectureAsync(LectureUploadDto dto, int userId)
        {
            // begin transaction
            await using var transaction = await _db.Database.BeginTransactionAsync();
            string? videoFile = null;
            try
            {
                // Save video file
                videoFile = await FileHelper.SaveImageAsync(dto.Video, _lectureFolder);
                if (string.IsNullOrEmpty(videoFile))
                    throw new Exception("Failed to save lecture video.");

                // Create Lecture entity
                var lecture = new Lecture
                {
                    Title = dto.Title,
                    Grade = dto.Grade,
                    VideoUrl =  $"/uploads/lectures/videos/{videoFile}",
                    Date = DateTime.UtcNow,
                    AdminId = userId,
                };
                await _db.Lectures.AddAsync(lecture);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return lecture.LectureId;
            }
            catch
            {
                await transaction.RollbackAsync();
                // delete file if saved
                if (!string.IsNullOrEmpty(videoFile))
                {
                    var full = Path.Combine(_lectureFolder, videoFile);
                    if (File.Exists(full)) File.Delete(full);
                }
                return -1;
            }
        }

        public async Task<bool> UploadQuizAsync(QuizUploadDto dto, int userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            string? quizFile = null;
            try
            {
                // Save quiz image
                quizFile = await FileHelper.SaveImageAsync(dto.Image, _quizFolder);
                if (string.IsNullOrEmpty(quizFile))
                    throw new Exception("Failed to save quiz image.");

                // Create Quiz entity
                var quiz = new Quiz
                {
                    Title = dto.Title,
                    QuizUrl = $"/uploads/quizzes/images/{quizFile}",
                    Date = DateTime.UtcNow,
                    AdminId = userId,
                    LectureId = dto.LectureId
                };
                await _db.Quizzes.AddAsync(quiz);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                if (!string.IsNullOrEmpty(quizFile))
                {
                    var full = Path.Combine(_quizFolder, quizFile);
                    if (File.Exists(full)) File.Delete(full);
                }
                return false;
            }
        }

        public async Task<List<LockedUserDto>> GetAllStudentAccountAsync()
        {
            var studentRoleName = "Student";

            var lockedStudents = await (from user in _db.Users
                                        join userRole in _db.UserRoles on user.Id equals userRole.UserId
                                        join role in _db.Roles on userRole.RoleId equals role.Id
                                        where role.Name == studentRoleName
                                        select new LockedUserDto
                                        {
                                            Id = user.Id,
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            PhoneNumber = user.PhoneNumber,
                                            Grade = user.Grade,
                                            IsLocked = user.IsLocked
                                        }).OrderByDescending(u => u.Grade).ThenBy(u => u.FirstName)
                                         .ToListAsync();

            return lockedStudents;
        }


        public async Task<bool> ToggleUserLockStatusAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
               ?? throw new ArgumentException("User not found", nameof(userId));

            user.IsLocked = !user.IsLocked;
            if (user.IsLocked)
            {

                // Revoke active token
                user.CurrentJwtToken = null;
                user.TokenExpiry = null;
            }
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return user.IsLocked;
        }

        public async Task<List<QuizzesAnswerDto>> QuizzesAnswersAsync()
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3);

            var submissions = await _db.QuizSubmissions
                .AsNoTracking()
                .Where(qs => qs.SubmissionDate >= threeDaysAgo)
                .Include(qs => qs.Quiz)
                    .ThenInclude(q => q.Lecture)
                .Include(qs => qs.Student)
                .Select(qs => new QuizzesAnswerDto
                {
                    FirstName    = qs.Student.FirstName,
                    LastName     = qs.Student.LastName,
                    PhoneNumber  = qs.Student.PhoneNumber,
                    Grade        = qs.Student.Grade ?? 0,
                    LectureTitle = qs.Quiz.Lecture.Title,
                    QuizAnwerURl = qs.AnswerUrl
                })
                .OrderBy(qs => qs.Grade)
                .ThenBy(qs => qs.FirstName)
                .ThenBy(qs => qs.LastName)
                .ToListAsync();

            return submissions;
        }


    }
}
