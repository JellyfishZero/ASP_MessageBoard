namespace ASP_MessageBoard.Services.DTOs
{
    public class LoginRequest
    {
        public string PhoneNumber { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}
