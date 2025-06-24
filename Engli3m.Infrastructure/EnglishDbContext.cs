using Engli3m.Domain.Enities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Engli3m.Infrastructure
{
    public class EnglishDbContext(DbContextOptions<EnglishDbContext> options) : IdentityDbContext<User, Role, int> (options)
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can go here
        }

    }
}