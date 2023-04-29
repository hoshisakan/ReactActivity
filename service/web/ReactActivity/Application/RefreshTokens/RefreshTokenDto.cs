using System.Text.Json.Serialization;

namespace Application.RefreshTokens
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }

        [JsonIgnore]
        public bool IsRevoked { get; set; }
        [JsonIgnore]
        public string AppUserId { get; set; }
    }
}