using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engli3m.Infrastructure.Services
{
    public class PaymentCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentCheckService> _logger;

        // ← Proper constructor for DI
        public PaymentCheckService(
            IServiceProvider serviceProvider,
            ILogger<PaymentCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                _logger.LogInformation("🔄 Checking for students to lock...");

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EnglishDbContext>();

                var studentsToLock = await db.Users
                    .Where(u => !u.IsLocked)
                    .ToListAsync(stoppingToken);

                if (studentsToLock.Any())
                {
                    studentsToLock.ForEach(u => u.IsLocked = true);
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("✅ Locked {Count} students", studentsToLock.Count);
                }
                else
                {
                    _logger.LogInformation("👍 No students to lock at this time.");
                }

                // For testing, wait 1 minute; in prod use 24 hours
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
