using Engli3m.Application.DTOs;
using Engli3m.Domain.Enums;

namespace Engli3m.Application.Interfaces
{
    public interface INotificationService
    {
        Task<string> SendToUserAsync(string token, string title, string body);
        Task<string> SendToGradeAsync(GradeLevel grade, string title, string body);
        Task<List<string>> SendToAllGradesAsync(string title, string body);
        Task SaveFcmTokenAsync(SaveFcmTokenDto dto);

    }
}
