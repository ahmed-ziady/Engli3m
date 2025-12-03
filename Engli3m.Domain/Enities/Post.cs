namespace Engli3m.Domain.Enities
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<PostMedia>? Media { get; set; }

        public ICollection<FavPost> FavPosts { get; set; } = [];
    }
}
