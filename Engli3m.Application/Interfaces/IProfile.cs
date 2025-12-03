using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Post;
using Engli3m.Application.DTOs.Profile;

namespace Engli3m.Application.Interfaces
{
    public interface IProfile
    {
        Task<ProfileResponseDto> GetProfileAsync(int userId);
        Task<bool> UpdatePhoneNumber(int userId, string phoneNumber);
        Task<bool> UpdateProfilePicture(int userId, ProfileImageDto profileImageDto);
        Task<string> ResetPasswordAsync(int userID, string password);
        Task<List<FavPostDto>> GetFavPostAsync(int userId);
        Task<IEnumerable<GetLectureProgressDto>> GetSubmittedProgressAsync(int studentId);
    }
}
