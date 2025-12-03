using Engli3m.Application.DTOs.Auth;
using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Profile;
using Engli3m.Application.DTOs.Quiz;
using Engli3m.Domain.Enums;

namespace Engli3m.Application.Interfaces
{
    public interface IAdminService
    {
        Task<Int32> UploadLectureAsync(LectureUploadDto dto, int userId);
        Task<bool> ActiveLectureAsync(int lectureId);
        Task<bool> UploadQuizAsync(QuizUploadDto quizUploadDto, int userId);
        Task<List<LockedUserDto>> GetAllStudentAccountAsync(GradeLevel gradeLevel);
        Task<bool> ToggleUserLockStatusAsync(int userId);
        Task<bool> MarkTheUserAsPaidAsync(int userId);
        Task<List<QuizzesAnswerDto>> QuizzesAnswersAsync();

        Task<List<LecturesDto>> GetLecturesByGrade(GradeLevel grade);
        Task<bool> EditLectureAsync(int lectureId, string lectureName);
        Task<bool> DeleteLecureAsync(int lectureId);

        Task<bool> SubmitDegreeAsync(int userId, double degree);
        Task<List<TopTenStudentsDto>> GetTopTenStudentsAsync(GradeLevel gradeLevel);
        Task<bool> ToggleUsersLockStatusAsync(bool toggleLock);
    }

}
