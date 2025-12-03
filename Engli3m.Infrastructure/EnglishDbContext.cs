using Engli3m.Domain.Enities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure
{
    public class EnglishDbContext(DbContextOptions<EnglishDbContext> options) : IdentityDbContext<User, Role, int>(options)
    {

        public DbSet<DeviceToken> DeviceTokens { get; set; }


        public DbSet<Lecture> Lectures { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizSubmission> QuizSubmissions { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<PostMedia> Media { get; set; } = null!;
        public DbSet<EQuiz> EQuizzes { get; set; } = null!;
        public DbSet<Questions> QuizQuestions { get; set; } = null!;
        public DbSet<QuestionsAnswer> QuestionsAnswers { get; set; } = null!;
        public DbSet<EQuizSubmission> EQuizSubmissions { get; set; } = null!;
        public DbSet<EQuestionSubmission> EQuestionSubmissions { get; set; } = null!;
        public DbSet<FavPost> FavPosts { get; set; }

        public DbSet<VideoProgress> VideoProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store enums as strings for readability
            modelBuilder.Entity<User>()
                .Property(u => u.Grade)
                .HasConversion<string>();

            modelBuilder.Entity<Lecture>()
                .Property(l => l.Grade)
                .HasConversion<string>();



            // Lecture relationships
            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Admin)
                .WithMany(u => u.LecturesAsAdmin)
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict);


            // Quiz relationships
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Lecture)
                .WithMany(l => l.Quizzes)
                .HasForeignKey(q => q.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Admin)
                .WithMany(u => u.QuizzesAsAdmin)
                .HasForeignKey(q => q.AdminId)
                .OnDelete(DeleteBehavior.Cascade);


            // QuizSubmission relationships
            modelBuilder.Entity<QuizSubmission>()
                .HasKey(s => s.QuizSubmissionId);

            modelBuilder.Entity<QuizSubmission>()
                .HasOne(s => s.Quiz)
                .WithMany(q => q.Submissions)
                .HasForeignKey(s => s.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizSubmission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.QuizSubmissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post relationships
            modelBuilder.Entity<Post>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Media)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // EQuiz
            modelBuilder.Entity<EQuiz>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<EQuiz>()
                .HasOne(e => e.User)
                .WithMany(u => u.EQuizzes)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EQuiz>()
                .HasMany(e => e.Questions)
                .WithOne(q => q.EQuiz)
                .HasForeignKey(q => q.EQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Questions
            modelBuilder.Entity<Questions>()
                .HasKey(q => q.Id);

            modelBuilder.Entity<Questions>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Questions>()
                .HasMany(q => q.EQuestionSubmissions)
                .WithOne(qs => qs.Question)
                .HasForeignKey(qs => qs.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); // 🔁 Prevent circular cascade

            // QuestionsAnswer
            modelBuilder.Entity<QuestionsAnswer>()
                .HasKey(a => a.Id);

            // EQuizSubmission
            modelBuilder.Entity<EQuizSubmission>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<EQuizSubmission>()
                .HasOne(s => s.EQuiz)
                .WithMany(q => q.Submissions)
                .HasForeignKey(s => s.EQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EQuizSubmission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.EQuizSubmissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EQuizSubmission>()
                .HasMany(s => s.EQuestionSubmissions)
                .WithOne(qs => qs.EQuizSubmission)
                .HasForeignKey(qs => qs.EQuizSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // EQuestionSubmission
            modelBuilder.Entity<EQuestionSubmission>()
                .HasKey(qs => qs.Id);

            // Already handled above: QuestionId → Restrict
            modelBuilder.Entity<FavPost>()
                .HasKey(fp => fp.Id);

            modelBuilder.Entity<FavPost>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.FavPosts)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavPost>()
                .HasOne(fp => fp.Post)
                .WithMany(p => p.FavPosts)
                .HasForeignKey(fp => fp.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<VideoProgress>()
                .HasKey(v => v.Id);
            modelBuilder.Entity<VideoProgress>()
                 .HasOne(vp => vp.Student)
                 .WithMany(u => u.VideoProgressRecords)
                 .HasForeignKey(vp => vp.StudentId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoProgress>()
                .HasOne(vp => vp.Video)
                .WithMany(l => l.VideoProgressRecords)
                .HasForeignKey(vp => vp.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }


}
