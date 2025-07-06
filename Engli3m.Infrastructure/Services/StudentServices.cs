using Engli3m.Application.DTOs;
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
                .Where(l => l.Grade == grade)
                .Include(l => l.Quizzes)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            var result = lectures.Select(l => new LectureWithQuizzesDto
            {
                LectureId = l.LectureId,
                Grade = l.Grade,
                LectureTitle = l.Title,
                VideoUrl = l.VideoUrl,
                Quizzes = l.Quizzes
                    .OrderBy(q => q.Date)
                    .Select(q => new QuizItemDto
                    {
                        QuizId = q.QuizId,
                        Title = q.Title,
                        ImageUrl = q.QuizUrl,
                    })
                    .ToList()
            })
            .ToList();

            return result;
        }
        public async Task<bool> SubmmitQuizAnswerAsync(SubmmitQuizDto dto, int id)
        {
            if (dto == null) return false;
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            string? quizFile = null;
            try
            {
                // Save quiz image
                quizFile = await FileHelper.SaveImageAsync(dto.AsnwerImage, _answerFolder);
                if (string.IsNullOrEmpty(quizFile))
                    throw new Exception("Failed to save quiz image.");

                // Create Quiz entity
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
