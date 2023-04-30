namespace API.DTOs
{
    public class ValidateTokenDto
    {
        public string JwtId { get; set; } = string.Empty;
        public bool IsValid { get; set; } = false;
        public bool IsExpired { get; set; } = false;
    }
}