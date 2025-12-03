using Engli3m.Application.DTOs.Auth;
using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Profile;
using Engli3m.Application.DTOs.Quiz;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Domain.Enums;
using Engli3m.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
namespace Engli3m.Infrastructure.Services
{
    public class AdminServices : IAdminService
    {
        private readonly EnglishDbContext _db;
        private readonly string _lectureFolder;
        private readonly string _quizFolder;
        private readonly INotificationService _notificationService;
        public AdminServices(EnglishDbContext db, INotificationService notificationService)
        {
            _db = db;
            _lectureFolder = Path.Combine("wwwroot", "uploads", "lectures", "videos");
            _quizFolder = Path.Combine("wwwroot", "uploads", "quizzes", "images");
            Directory.CreateDirectory(_lectureFolder);
            Directory.CreateDirectory(_quizFolder);
            _notificationService = notificationService;
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
                    IsActive = false,
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

        public async Task<List<LockedUserDto>> GetAllStudentAccountAsync(GradeLevel grade)
        {
            var studentRoleName = "Student";

            var filteredUsers = _db.Users.Where(u => u.Grade == grade);

            var lockedStudents = await (from user in filteredUsers
                                        join userRole in _db.UserRoles on user.Id equals userRole.UserId
                                        join role in _db.Roles on userRole.RoleId equals role.Id
                                        where role.Name == studentRoleName
                                        orderby user.FirstName, user.LastName, user.IsLocked
                                        select new LockedUserDto
                                        {
                                            Id = user.Id,
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            PhoneNumber = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "01200000000" : user.PhoneNumber,
                                            Grade = user.Grade,
                                            IsPayed = user.IsPayed,
                                            IsLocked = user.IsLocked
                                        }).ToListAsync();

            return lockedStudents;
        }


        public async Task<bool> ToggleUserLockStatusAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new ArgumentException("User not found", nameof(userId));

            // Toggle lock status
            user.IsLocked = !user.IsLocked;

            if (user.IsLocked)
            {
                // 🔒 Lock account and clear token info
                user.IsPayed = false;
                user.CurrentJwtToken = null;
                user.TokenExpiry = null;
            }

            // EF already tracks the user, so just save
            await _db.SaveChangesAsync();

            // 🔔 Try to send a notification, but don’t let it break the API
            try
            {
                if (!string.IsNullOrWhiteSpace(user.FcmToken))
                {
                    string title = user.IsLocked ? "تم قفل الحساب" : "تم فتح الحساب";
                    string body = user.IsLocked
                        ? "تم قفل حسابك مؤقتًا. يرجى التواصل مع الميستر لمزيد من التفاصيل."
                        : "تم إعادة فتح حسابك بنجاح. يمكنك الآن استخدام التطبيق بشكل طبيعي.";

                    await _notificationService.SendToUserAsync(user.FcmToken, title, body);
                }
            }
            catch
            {
                // Ignore notification errors — don’t crash the API
            }

            return user.IsLocked;
        }


        public async Task<List<QuizzesAnswerDto>> QuizzesAnswersAsync()
        {
            var threeDaysAgo = DateTime.UtcNow.AddDays(-300);

            return await _db.QuizSubmissions
                .AsNoTracking()
                .Where(qs => qs.SubmissionDate >= threeDaysAgo)
                .OrderBy(qs => qs.Student.Grade ?? 0).ThenByDescending(l => l.SubmissionDate)
                .ThenBy(qs => qs.Student.FirstName)
                .ThenBy(qs => qs.Student.LastName)
                .Select(qs => new QuizzesAnswerDto
                {
                    UserId =qs.StudentId,
                    FirstName    = qs.Student.FirstName,
                    LastName     = qs.Student.LastName,
                    PhoneNumber  = qs.Student.PhoneNumber,
                    Grade        = qs.Student.Grade ?? 0,
                    LectureTitle = qs.Quiz.Lecture.Title,
                    QuizAnwerURl = qs.AnswerUrl
                })
                .ToListAsync();
        }

        public async Task<bool> MarkTheUserAsPaidAsync(int userId)
        {
            var user = _db.Users.Find(userId)
                ?? throw new ArgumentException("User not موجود", nameof(userId));

            user.IsPayed = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            // 🔔 إرسال إشعار للمستخدم بعد الدفع
            if (!string.IsNullOrWhiteSpace(user.FcmToken))
            {
                var title = " تم تأكيد الدفع";
                var body = "شكراً لك! تم تأكيد عملية الدفع بنجاح، يمكنك الآن الوصول إلى المحتوى بالكامل.";
                await _notificationService.SendToUserAsync(user.FcmToken, title, body);
            }

            return true;
        }
        public async Task<List<LecturesDto>> GetLecturesByGrade(GradeLevel grade)
        {
            var lectures = await _db.Lectures
                .Where(l => l.Grade == grade).OrderByDescending(l => l.IsActive).ThenByDescending(l => l.Date)
                .Select(l => new LecturesDto
                {
                    LectureId = l.LectureId,
                    Grade = l.Grade,
                    LectureTitle = l.Title,
                    VideoUrl = l.VideoUrl,
                    IsActive = l.IsActive,
                })
                .ToListAsync();

            return lectures;
        }

        public async Task<bool> EditLectureAsync(int lectureId, string lectureName)
        {
            var lecture = await _db.Lectures.Where(l => l.LectureId == lectureId).FirstOrDefaultAsync();
            if (lecture == null)
                return false;
            lecture.Title= lectureName;
            _db.Update(lecture);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteLecureAsync(int lectureId)
        {
            var lecture = await _db.Lectures
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Submissions)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null)
                return false;

            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                // Delete lecture video file
                var videoPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    lecture.VideoUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
                if (File.Exists(videoPath))
                    File.Delete(videoPath);

                // Loop through quizzes
                foreach (var quiz in lecture.Quizzes)
                {
                    // Delete quiz image file
                    var quizImagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        quiz.QuizUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );
                    if (File.Exists(quizImagePath))
                        File.Delete(quizImagePath);

                    // Delete submissions
                    _db.QuizSubmissions.RemoveRange(quiz.Submissions);

                    // Delete quiz
                    _db.Quizzes.Remove(quiz);
                }

                // Delete lecture
                _db.Lectures.Remove(lecture);

                await _db.SaveChangesAsync();
                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> ActiveLectureAsync(int lectureId)
        {
            var lecture = await _db.Lectures.Where(l => l.LectureId == lectureId).FirstOrDefaultAsync();
            if (lecture == null)
                return false;
            lecture.IsActive= !lecture.IsActive;

            _db.Update(lecture);
            _db.SaveChanges();

            if (lecture.IsActive)
            {
                await _notificationService.SendToGradeAsync(
                    lecture.Grade, // enum value
                    "📢 تم اضافة حصة جديدة",
                    $"\"{lecture.Title}\"، يمكنك الآن مشاهدتها."
                );
            }
            return lecture.IsActive;
        }

        public async Task<bool> SubmitDegreeAsync(int userId, double degree)
        {
            var updated = await _db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.NetScore, u => u.NetScore  + degree)
                );


            var token = _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FcmToken)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(token))
            {
                string title = "📊 نتيجتك في الامتحان";
                string body;

                if (degree >= 9)
                {
                    body = $"🔥 ممتاز يا بطل!\nجبت {degree} من 10 💯\nاستمر كده وهتبقى اشطر واحد !";
                }
                else if (degree >= 7)
                {
                    body = $"💪 شغل كويس!\nجبت {degree} من 10\nكمّل كده !";
                }
                else if (degree >= 5)
                {
                    body = $"✍️ محتاج تراجع شوية\nجبت {degree} من 10\nبس انت قدها وهتتحسن!";
                }
                else
                {
                    body = $"😔 مش مشكلة خالص\nجبت {degree} من 10\nحاول تاني وهتتفوق بإذن الله!";
                }

                await _notificationService.SendToUserAsync(token, title, body);
            }
            return updated > 0;
        }

        public async Task<List<TopTenStudentsDto>> GetTopTenStudentsAsync(GradeLevel gradeLevel)
        {
            var topTenStudents = await _db.Users
                .Where(u => u.Grade == gradeLevel && u.NetScore > 0)
                .OrderByDescending(u => u.NetScore)
                .ThenBy(u => u.LastName)
                .Take(10)
                .ToListAsync();

            if (topTenStudents.Count==0)
                return [];

            var cutoff = topTenStudents.Last().NetScore;

            var students = await _db.Users
                .Where(u => u.Grade == gradeLevel && u.NetScore >= cutoff)
                .OrderByDescending(u => u.NetScore)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var rankedList = students
                .GroupBy(s => s.NetScore)
                .OrderByDescending(g => g.Key)
                .SelectMany((g, rankIndex) => g.Select(s => new TopTenStudentsDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    GradeLevel = s.Grade ?? gradeLevel,
                    NetScore = s.NetScore,
                    Rank = rankIndex + 1
                }))
                .ToList();

            return rankedList;
        }

        public async Task<bool> ToggleUsersLockStatusAsync(bool toggleLock)
        {
            // 1. Bulk update in ONE SQL statement
            await (
                from user in _db.Users
                join userRole in _db.UserRoles on user.Id equals userRole.UserId
                join role in _db.Roles on userRole.RoleId equals role.Id
                where role.Name != "Admin"
                select user
            )
            .ExecuteUpdateAsync(update =>
                update.SetProperty(u => u.IsLocked, toggleLock)
            );

            // 2. Send one notification per grade 
            string title = toggleLock
                ? "تم قفل الحسابات"
                : "تم فتح الحسابات";

            string body = toggleLock
                ? "تم قفل حسابات جميع الطلاب مؤقتًا. يرجى التواصل مع الميستر لمزيد من التفاصيل."
                : "تم فتح حسابات جميع الطلاب ويمكنهم الآن استخدام التطبيق بشكل طبيعي.";

            await _notificationService.SendToAllGradesAsync(title, body);

            return toggleLock;
        }




    }
}
