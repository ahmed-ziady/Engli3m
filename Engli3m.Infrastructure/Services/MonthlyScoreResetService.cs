using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Engli3m.Infrastructure.Services
{
    public class MonthlyScoreResetService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyScoreResetService> _logger;

        public MonthlyScoreResetService(IServiceScopeFactory scopeFactory, ILogger<MonthlyScoreResetService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0);
            if (now > nextRun) nextRun = nextRun.AddDays(1);

            var initialDelay = nextRun - now;
            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromDays(1));

            _logger.LogInformation("📅 MonthlyScoreResetService scheduled. Next run: {NextRun}", nextRun);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                var today = DateTime.Today;
                if (today.Day != 1) return; // ✅ Only run first day of month

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EnglishDbContext>();

                var users = await db.Users.ToListAsync();
                foreach (var user in users)
                {
                    user.NetScore = 0;
                }

                await db.SaveChangesAsync();
                _logger.LogInformation("✅ NetScores reset on {Date}", today.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during monthly NetScore reset");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
