namespace ProjectManagementSystem.Models.API
{
    public class TokenModel
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }
} 