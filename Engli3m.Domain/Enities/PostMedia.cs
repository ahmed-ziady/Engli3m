namespace Engli3m.Domain.Enities
{
    public class PostMedia
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
        // Additional properties can be added here, such as media type, size, etc.
    }
}
