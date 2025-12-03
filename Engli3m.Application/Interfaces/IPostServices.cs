using Engli3m.Application.DTOs.Post;

namespace Engli3m.Application.Interfaces
{
    public interface IPostServices
    {
        public Task<bool> CreatePostAsync(int userID, CreatePostDto createPostDto);
        public Task<bool> DeletePostAsync(int postId);
        public Task<List<PostResponseDto>> GetAllPostAsync(int pageNumber, int pageSize, int currentUserId);
        public Task<bool> MarkPostFavAsync(int postId, int userId );

    }
}
