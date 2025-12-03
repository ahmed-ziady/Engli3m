namespace Engli3m.Domain.Enities
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
