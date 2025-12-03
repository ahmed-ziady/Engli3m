using Engli3m.Application.DTOs.EQuiz;
using Engli3m.Domain.Enums;

namespace Engli3m.Application.Interfaces
{
    public interface IEQuizServices
    {
        Task CreateEQuizAsync(CreateEQuizDto createEQuizDto, int userID);
        Task<bool> ActiveEQuizByIdAsync(int id);
        Task<List<EQuizResponeDto>> GetAllEQuizzesAsync();
        Task<List<EQuizUserResponeDto>> GetEQuizByGradeAsync(GradeLevel gradeLevel, int userId);
        Task<bool> DeleteEQuizAsync(int eQuizId);
        Task<bool> SubmitQuizAsync(EQuizSubmissionDto dto, int studentId);
        Task<EQuizResultDto> GetQuizResultAsync(int eQuizId, int userId);
        Task<List<EQuizResultDto>> GetAllQuizResultAsync(int userId);
    }

}
