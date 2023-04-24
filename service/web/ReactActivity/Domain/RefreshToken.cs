using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("RefreshTokens", Schema = "reactactivity")]
    public class RefreshToken
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpirationTime { get; set; }
    }
}