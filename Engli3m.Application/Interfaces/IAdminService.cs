using Engli3m.Application.DTOs;

namespace Engli3m.Application.Interfaces
{
    public interface IAdminService
    {
        Task<Int32> UploadLectureAsync(LectureUploadDto dto, int userId);
        Task<bool> UploadQuizAsync(QuizUploadDto quizUploadDto, int userId);
        Task<List<LockedUserDto>> GetAllStudentAccountAsync();
        Task<bool> ToggleUserLockStatusAsync(int userId);
        Task<List<QuizzesAnswerDto>> QuizzesAnswersAsync();
    }

}
