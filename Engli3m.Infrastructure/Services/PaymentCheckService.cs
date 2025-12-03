using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engli3m.Infrastructure.Services
{
    public class PaymentCheckService(
        IServiceProvider serviceProvider,
        ILogger<PaymentCheckService> logger
    ) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<PaymentCheckService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentCheckService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1) Compute next run: last day of current month at 00:00
                    var now = DateTime.Now;
                    var year = now.Year;
                    var month = now.Month;
                    var lastDay = DateTime.DaysInMonth(year, month);
                    var runTime = new DateTime(year, month, lastDay, 0, 0, 0);

                    // If we've passed this month's run time, schedule for next month
                    if (now >= runTime)
                    {
                        // Advance to next month
                        var nextMonth = now.AddMonths(1);
                        year  = nextMonth.Year;
                        month = nextMonth.Month;
                        lastDay = DateTime.DaysInMonth(year, month);
                        runTime = new DateTime(year, month, lastDay, 0, 0, 0);
                    }

                    var delay = runTime - now;
                    _logger.LogInformation(
                        "PaymentCheckService sleeping for {Days}d {Hours}h {Minutes}m until next run at {RunTime}.",
                        delay.Days, delay.Hours, delay.Minutes, runTime);

                    await Task.Delay(delay, stoppingToken);

                    // 2) Perform the lock/reset
                    _logger.LogInformation("Running month‑end payment check at {Time}.", DateTime.Now);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EnglishDbContext>();

                    // Lock all unpaid students
                    var unpaid = await (
                        from u in db.Users
                        join ur in db.UserRoles on u.Id equals ur.UserId
                        join r in db.Roles on ur.RoleId equals r.Id
                        where r.Name == "Student" && !u.IsPayed && !u.IsLocked
                        select u
                    ).ToListAsync(stoppingToken);

                    unpaid.ForEach(u => u.IsLocked = true);
                    _logger.LogInformation("Locked {Count} unpaid students.", unpaid.Count);

                    // Reset the Payed flag for all students
                    var allStudents = await (
                        from u in db.Users
                        join ur in db.UserRoles on u.Id equals ur.UserId
                        join r in db.Roles on ur.RoleId equals r.Id
                        where r.Name == "Student"
                        select u
                    ).ToListAsync(stoppingToken);

                    allStudents.ForEach(u => u.IsPayed = false);
                    _logger.LogInformation("Reset payment flag for {Count} students.", allStudents.Count);

                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Month‑end payment check completed.");
                }
                catch (OperationCanceledException)
                {
                    // host is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during PaymentCheckService execution");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("PaymentCheckService is stopping.");
        }
    }
}