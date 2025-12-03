using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enums;
using Engli3m.Infrastructure;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

public class FirebaseNotificationService : INotificationService
{
    private readonly EnglishDbContext dbContext;

    public FirebaseNotificationService(EnglishDbContext dbContext)
    {
        this.dbContext = dbContext;
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("englizm--notifications-firebase-adminsdk-fbsvc-1e88498239.json")
            });
        }
    }

    public async Task<string> SendToUserAsync(string token, string title, string body)
    {
        var message = new Message()
        {
            Token = token,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = new Dictionary<string, string>
            {
                { "title", title },
                { "body", body },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    // ✅ حفظ FCM Token للمستخدم
    public async Task SaveFcmTokenAsync(SaveFcmTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            throw new ArgumentException("Token cannot be empty");

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId)
            ?? throw new KeyNotFoundException("User not found");

        user.FcmToken = dto.Token;
        await dbContext.SaveChangesAsync();
    }

    public async Task<string> SendToGradeAsync(GradeLevel grade, string title, string body)
    {
        string topic = grade.ToString();

        var message = new Message()
        {
            Topic = topic,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = new Dictionary<string, string>
            {
                { "title", title },
                { "body", body },
                { "grade", topic },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task<List<string>> SendToAllGradesAsync(string title, string body)
    {
        var results = new List<string>();

        foreach (GradeLevel grade in Enum.GetValues(typeof(GradeLevel)))
        {
            var message = new Message()
            {
                Topic = grade.ToString(),
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "title", title },
                    { "body", body },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            };

            string messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            results.Add($"Grade: {grade} → MessageId: {messageId}");
        }

        return results;
    }
}