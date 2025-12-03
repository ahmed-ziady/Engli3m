namespace Engli3m.Domain.Enities
{
    public class VideoProgress
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int VideoId { get; set; }
        public int WatchedSeconds { get; set; }
        public bool IsWatched { get; set; }
        public Lecture Video { get; set; } = null!;
        public User Student { get; set; } = null!;
    }

}
