using Engli3m.Application.DTOs;

namespace Engli3m.Application.Interfaces
{
    public interface IStudentService
    {
        Task<List<LectureWithQuizzesDto>> GetAllLecturesAndQuizzes(int id);
        public Task <bool> SubmmitQuizAnswerAsync(SubmmitQuizDto answer, int id);

    }
}
