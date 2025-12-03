using Engli3m.Application.DTOs.EQuiz;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure.Services
{
    public class EQuizServices(EnglishDbContext dbContext, INotificationService _notificationService) : IEQuizServices
    {
        private readonly EnglishDbContext _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        private static bool IsWrittenAnswerCorrect(string? correctAnswer, string? studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(correctAnswer) || string.IsNullOrWhiteSpace(studentAnswer))
                return false;

            correctAnswer = correctAnswer.Trim().ToLower();
            studentAnswer = studentAnswer.Trim().ToLower();

            // must be same length (strict)
            if (correctAnswer.Length != studentAnswer.Length)
                return false;

            // exact match
            if (correctAnswer == studentAnswer)
                return true;

            // allow exactly one differing character
            int differences = 0;
            for (int i = 0; i < correctAnswer.Length; i++)
            {
                if (correctAnswer[i] != studentAnswer[i])
                {
                    differences++;
                    if (differences > 1) return false;
                }
            }

            return differences == 1;
        }

        public async Task CreateEQuizAsync(CreateEQuizDto dto, int userId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            await using var tx = await _db.Database.BeginTransactionAsync();

            var eQuiz = new EQuiz
            {
                UserID = userId,
                Title = dto.Title,
                Grade = dto.Grade,
                Duration = dto.Duration,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                TotalPoints = dto.Questions?.Sum(q => q.Points) ?? 0,
                Questions = []
            };

            if (dto.Questions != null)
            {
                foreach (var qDto in dto.Questions)
                {
                    ArgumentNullException.ThrowIfNull(qDto);

                    var question = new Questions
                    {
                        Text = qDto.QuestionContent,
                        Explanation = qDto.Explanation,
                        Points = qDto.Points,
                        Type = qDto.Type,
                        Answers = []
                    };

                    if (question.Type == QuestionType.MCQ || question.Type == QuestionType.TrueFalse)
                    {
                        if (qDto.Answers == null || qDto.Answers.Count == 0)
                            throw new ArgumentException($"{question.Type} requires at least one answer.");

                        if (question.Type == QuestionType.TrueFalse
                            && qDto.Answers.Count(a => a.IsCorrect) != 1)
                            throw new ArgumentException("True/False must have exactly one correct option.");

                        question.Answers = [.. qDto.Answers
                            .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Answer))
                            .Select(aDto => new QuestionsAnswer
                            {
                                Text = aDto.Answer.Trim(),
                                IsCorrect = aDto.IsCorrect,
                                Explanation = aDto.Explanation
                            })];
                    }
                    // inside CreateEQuizAsync when creating each question:

                    else if (question.Type == QuestionType.Written)
                    {
                        // Require at least one sample correct text for auto-grading.
                        if (qDto.Answers == null || qDto.Answers.Count == 0)
                            throw new ArgumentException("Written questions require at least one sample (correct) answer for auto-grading.");

                        question.Answers = [.. qDto.Answers
                            .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Answer))
                            .Select(aDto => new QuestionsAnswer
                            {
                                // Ensure your QuestionsAnswer entity has a 'Text' property.
                                Text = aDto.Answer.Trim(),
                                IsCorrect = true,
                                Explanation = aDto.Explanation
                            })];
                    }


                    eQuiz.Questions.Add(question);
                }
            }

            _db.EQuizzes.Add(eQuiz);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<List<EQuizResponeDto>> GetAllEQuizzesAsync()
        {
            var quizzes = await _db.EQuizzes
                .AsNoTracking()
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .AsSplitQuery().OrderByDescending(s => s.CreatedAt)
                .ThenByDescending(q => q.IsActive)
                .ToListAsync();

            return [.. quizzes.Select(q => new EQuizResponeDto
            {
                EQuizId = q.Id,
                Title = q.Title,
                Grade = q.Grade,
                IsActive = q.IsActive,
                Duration = q.Duration,
                TotalPoints = q.TotalPoints,
                Questions = [.. q.Questions.Select(qs => new QuestionResponseDto
                {
                    QuestionId = qs.Id,
                    QuestionContent = qs.Text,
                    Explanation = qs.Explanation,
                    Points = qs.Points,
                    Type = qs.Type,
                    Answers = qs.Answers?.Select(a => new QuestionAnswersResponseDto
                    {
                        AnswerId = a.Id,
                        Answer = a.Text,
                        IsCorrect = a.IsCorrect,
                        Explanation = a.Explanation
                    }).ToList() ?? []
                })]
            })];
        }

        public async Task<bool> ActiveEQuizByIdAsync(int id)
        {
            var eQuiz = await _db.EQuizzes.FindAsync(id);
            if (eQuiz == null)
                return false;

            if (!eQuiz.IsActive)
            {
                eQuiz.IsActive = true;
                _db.EQuizzes.Update(eQuiz);
                await _db.SaveChangesAsync();

                var title = " اختبار إلكتروني جديد";
                var body = $" \"{eQuiz.Title}\"، يمكنك الآن الدخول وحل الأسئلة.";
                await _notificationService.SendToGradeAsync(eQuiz.Grade, title, body);


                return true;
            }

            return false;
        }

        public async Task<List<EQuizUserResponeDto>> GetEQuizByGradeAsync(GradeLevel gradeLevel, int userId)
        {
            var quizList = await _db.EQuizzes
                .Where(q => q.Grade == gradeLevel && q.IsActive
                    && !_db.EQuizSubmissions.Any(s => s.EQuizId == q.Id && s.StudentId == userId)) // ✅ استبعاد الكويزات اللي الطالب حلها
                .AsNoTracking()
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .AsSplitQuery()
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            if (quizList == null || quizList.Count == 0)
                throw new ArgumentException($"  لا يوجد اختبارات بعد, انتظر قليلا {gradeLevel}", nameof(gradeLevel));

            return [.. quizList.Select(quiz => new EQuizUserResponeDto
    {
        EQuizId = quiz.Id,
        Title = quiz.Title,
        Grade = quiz.Grade ,
        IsActive = quiz.IsActive,
        TotalPoints = quiz.TotalPoints,
        Duration = quiz.Duration,
        Questions = [.. quiz.Questions.Select(qs => new QuestionUserResponseDto
        {
            QuestionId = qs.Id,
            QuestionContent = qs.Text,
            Explanation = qs.Explanation,
            Points = qs.Points,
            Type = qs.Type,
            Answers = qs.Answers?.Select(a => new QuestionAnswersUserResponseDto
            {
                AnswerId = a.Id,
                Answer = a.Text,
                Explanation = a.Explanation
            }).ToList() ?? []
        })]
    })];
        }

        public async Task<bool> DeleteEQuizAsync(int eQuizId)
        {
            var equiz = await _db.EQuizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.Submissions)
                    .ThenInclude(s => s.EQuestionSubmissions)
                .FirstOrDefaultAsync(q => q.Id == eQuizId);

            if (equiz == null)
                return false;

            // Remove all EQuestionSubmissions first
            foreach (var submission in equiz.Submissions.ToList())
            {
                _db.EQuestionSubmissions.RemoveRange(submission.EQuestionSubmissions);
            }

            // Remove submissions
            _db.EQuizSubmissions.RemoveRange(equiz.Submissions);

            // Remove answers (cascade may handle this, but explicit is safer)
            _db.QuestionsAnswers.RemoveRange(equiz.Questions.SelectMany(q => q.Answers));

            // Remove questions
            _db.QuizQuestions.RemoveRange(equiz.Questions);

            // Finally remove the EQuiz
            _db.EQuizzes.Remove(equiz);

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SubmitQuizAsync(EQuizSubmissionDto dto, int studentId)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var quiz = await _db.EQuizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .AsSplitQuery()
                .FirstOrDefaultAsync(q => q.Id == dto.EQuizId)??throw new ArgumentException("Quiz not found.");

            if (await _db.EQuizSubmissions.AnyAsync(s => s.EQuizId == quiz.Id && s.StudentId == studentId))
                throw new InvalidOperationException("Student already submitted this quiz.");

            var submission = new EQuizSubmission
            {
                EQuizId = quiz.Id,
                StudentId = studentId,
                SubmittedAt = DateTime.UtcNow,
                EQuestionSubmissions = []
            };

            double totalScore = 0;

            // Begin a transaction to ensure submission + question-submissions are atomic
            await using var tx = await _db.Database.BeginTransactionAsync();

            foreach (var qDto in dto.Questions ?? Enumerable.Empty<EQuestionSubmissionDto>())
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == qDto.QuestionId);
                if (question == null) continue;

                var qSub = new EQuestionSubmission
                {
                    QuestionId = question.Id,
                    SelectedAnswerId = qDto.SelectedAnswerId,
                    WrittenAnswer = qDto.WrittenAnswer
                };

                // MCQ / TrueFalse grading
                if (question.Type == QuestionType.MCQ || question.Type == QuestionType.TrueFalse)
                {
                    var correctAnswer = question.Answers?.FirstOrDefault(a => a.IsCorrect);
                    if (correctAnswer != null && qDto.SelectedAnswerId.HasValue && correctAnswer.Id == qDto.SelectedAnswerId.Value)
                    {
                        qSub.EarnedPoints = question.Points;
                    }
                    else
                    {
                        qSub.EarnedPoints = 0;
                    }
                }
                // Written grading
                else if (question.Type == QuestionType.Written)
                {
                    var correctWritten = question.Answers?.FirstOrDefault(a => a.IsCorrect)?.Text
                                         ?? question.Answers?.FirstOrDefault()?.Text ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(correctWritten) && IsWrittenAnswerCorrect(correctWritten, qDto.WrittenAnswer))
                    {
                        qSub.EarnedPoints = question.Points;
                    }
                    else
                    {
                        qSub.EarnedPoints = 0;
                    }
                }

                totalScore += qSub.EarnedPoints;
                submission.EQuestionSubmissions.Add(qSub);
            }

            submission.Score = totalScore;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == studentId)
           ?? throw new InvalidOperationException("المستخدم غير موجود");

            user.NetScore += totalScore;

            _db.Users.Update(user);

            _db.EQuizSubmissions.Add(submission);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<EQuizResultDto> GetQuizResultAsync(int eQuizId, int studentId)
        {
            return await _db.EQuizSubmissions
             .Where(q => q.EQuizId == eQuizId && q.StudentId == studentId)
                .AsNoTracking()
              .Select(q => new EQuizResultDto
              {
                  QuizTitle = q.EQuiz.Title,
                  Score = q.Score,
                  Questions = q.EQuestionSubmissions.Select(a => new QuestionResultDto
                  {
                      QuestionText = a.Question.Text,
                      SubmittedAnswer = a.WrittenAnswer
                          ?? (a.SelectedAnswer != null ? a.SelectedAnswer.Text : string.Empty),
                      CorrectAnswer = a.Question.Answers
                          .Where(ans => ans.IsCorrect)
                          .Select(ans => ans.Text)
                          .FirstOrDefault() ?? string.Empty,
                      Points = a.EarnedPoints,
                      IsCorrectAnswer = a.EarnedPoints > 0,
                      AnswerExplanation = a.Question.Explanation
                          ?? a.Question.Answers
                              .Where(ans => ans.IsCorrect)
                              .Select(ans => ans.Explanation)
                              .FirstOrDefault()
                  }).ToList()
              })
    .FirstOrDefaultAsync()
    ?? throw new InvalidOperationException("لا يوجد نتيجة");

        }
        public async Task<List<EQuizResultDto>> GetAllQuizResultAsync(int userId)
        {
            return await _db.EQuizSubmissions
                .Where(q => q.StudentId == userId)
                .OrderByDescending(q => q.SubmittedAt)
                .Select(q => new EQuizResultDto
                {
                    QuizTitle = q.EQuiz.Title,
                    Score = q.Score,
                    Questions = q.EQuestionSubmissions.Select(a => new QuestionResultDto
                    {
                        QuestionText = a.Question.Text,
                        SubmittedAnswer = a.WrittenAnswer
                  ?? (a.SelectedAnswer != null ? a.SelectedAnswer.Text : string.Empty),
                        CorrectAnswer = a.Question.Answers
                        .Where(ans => ans.IsCorrect)
                        .Select(ans => ans.Text)
                        .FirstOrDefault() ?? string.Empty,
                        Points = a.EarnedPoints,
                        IsCorrectAnswer = a.EarnedPoints > 0,
                        AnswerExplanation = a.Question.Answers
                            .Where(ans => ans.IsCorrect)
                            .Select(ans => ans.Explanation)
                            .FirstOrDefault() ?? string.Empty
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
