namespace WebApi.DTOs.Authentication
{
    public class TokenResponse
    {
        public string TokenType { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string ExpiresIn { get; set; } = null!;
    }
}
