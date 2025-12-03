using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Quiz;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure.Services
{
    public class StudentServices : IStudentService
    {
        private readonly EnglishDbContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly string _answerFolder;

        public StudentServices(
            EnglishDbContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;

            _answerFolder  = Path.Combine("wwwroot", "uploads", "answers", "images");

            Directory.CreateDirectory(_answerFolder);
        }

        public async Task<List<LectureWithQuizzesDto>> GetAllLecturesAndQuizzes(int userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new ArgumentException($"No user with ID {userId}");

            if (user.Grade == null)
                return [];

            var grade = user.Grade.Value;

            var lectures = await dbContext.Lectures
                .AsNoTracking()
                .Where(l => l.Grade == grade && l.IsActive ==true)
                .Include(l => l.Quizzes)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            var result = lectures.Select(l => new LectureWithQuizzesDto
            {
                LectureId = l.LectureId,
                Grade = l.Grade,
                LectureTitle = l.Title,
                VideoUrl = l.VideoUrl,
                Quizzes = [.. l.Quizzes
                    .OrderBy(q => q.Date)
                    .Select(q => new QuizItemDto
                    {
                        QuizId = q.QuizId,
                        Title = q.Title,
                        ImageUrl = q.QuizUrl,
                    })]
            })
            .ToList();

            return result;
        }

        public async Task SetVideoProgress(int userId, LectureProgressDto lectureProgressDto)
        {
            try
            {
                var existingProgress = await dbContext.VideoProgress
                    .FirstOrDefaultAsync(vp => vp.StudentId == userId && vp.VideoId == lectureProgressDto.VideoId);

                if (existingProgress != null)
                {
                    existingProgress.WatchedSeconds = Math.Max(existingProgress.WatchedSeconds, lectureProgressDto.WatchedSeconds);
                }
                else
                {
                    await dbContext.VideoProgress.AddAsync(new VideoProgress
                    {
                        StudentId = userId,
                        VideoId = lectureProgressDto.VideoId,
                        WatchedSeconds = lectureProgressDto.WatchedSeconds,
                        IsWatched = lectureProgressDto.IsWatched


                    });
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to submit the progress right now. Please try again later.", ex);
            }
        }


        public async Task<bool> SubmmitQuizAnswerAsync(SubmmitQuizDto dto, int id)
        {
            if (dto == null) return false;

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            string? quizFile = null;

            try
            {
                // 🔒 Lock check inside transaction to avoid race condition
                bool alreadySubmitted = await dbContext.QuizSubmissions
                    .AnyAsync(q => q.QuizId == dto.QuizId && q.StudentId == id);

                if (alreadySubmitted)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // ✅ Save quiz image
                quizFile = await FileHelper.SaveImageAsync(dto.AsnwerImage, _answerFolder);
                if (string.IsNullOrEmpty(quizFile))
                    throw new Exception("لم يتم ارسال الاجابة، الرجاء إعادة المحاولة.");

                // ✅ Create submission
                var quizSubmission = new QuizSubmission
                {
                    QuizId = dto.QuizId,
                    AnswerUrl = $"/uploads/answers/images/{quizFile}",
                    SubmissionDate = DateTime.UtcNow,
                    StudentId = id,
                };

                await dbContext.QuizSubmissions.AddAsync(quizSubmission);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(quizFile))
                {
                    var full = Path.Combine(_answerFolder, quizFile);
                    if (File.Exists(full)) File.Delete(full);
                }

                return false;
            }
        }

    }
}
