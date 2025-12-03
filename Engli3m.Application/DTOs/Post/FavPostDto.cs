namespace Engli3m.Application.DTOs.Post
{
    public class FavPostDto
    {
        public int PostId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ProfilePictureUrl { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<string> MediaUrls { get; set; } = [];

    }
}
