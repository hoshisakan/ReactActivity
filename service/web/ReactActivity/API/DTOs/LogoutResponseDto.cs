namespace API.DTOs
{
    public class LogoutResponseDto
    {
        public string Message { get; set; }
        public bool IsLogout { get; set; }
        public string Error { get; set; }
    }
}