using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Quiz;

namespace Engli3m.Application.Interfaces
{
    public interface IStudentService
    {
        Task<List<LectureWithQuizzesDto>> GetAllLecturesAndQuizzes(int id);
        public Task<bool> SubmmitQuizAnswerAsync(SubmmitQuizDto answer, int id);

        public Task SetVideoProgress(int userId, LectureProgressDto lectureProgressDto);

    }
}
