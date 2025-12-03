namespace Engli3m.Application.DTOs.Lecture
{
    public class GetLectureProgressDto
    {
        public bool IsWached { get; set; }
        public string LectureTitle { get; set; } = string.Empty;
        public int Seconds { get; set; }
    }
}
