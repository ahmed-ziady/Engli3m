namespace Engli3m.Domain.Enities
{
    public class FavPost
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public User User { get; set; } = null!;

        public Post Post { get; set; } = null!;

    }
}
