using Engli3m.Domain.Enities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure
{
    public class EnglishDbContext(DbContextOptions<EnglishDbContext> options) : IdentityDbContext<User, Role, int>(options)
    {



        public DbSet<Lecture> Lectures { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizSubmission> QuizSubmissions { get; set; } = null!;

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
                .OnDelete(DeleteBehavior.Restrict);


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
                .OnDelete(DeleteBehavior.Restrict);
        }
    }


}
