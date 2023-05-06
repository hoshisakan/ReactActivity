using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("RefreshTokens", Schema = "reactactivity")]
    public class RefreshToken
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}