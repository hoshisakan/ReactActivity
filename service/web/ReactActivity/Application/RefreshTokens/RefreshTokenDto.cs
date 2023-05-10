using System.Text.Json.Serialization;

namespace Application.RefreshTokens
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Revoked { get; set; }
        [JsonIgnore]
        public string AppUserId { get; set; }
    }
}