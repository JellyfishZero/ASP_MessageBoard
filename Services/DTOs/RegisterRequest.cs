namespace ASP_MessageBoard.Services.DTOs
{
    public class RegisterRequest
    {
        public string UserName { get; init; } = string.Empty;

        public string PhoneNumber { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? Biography { get; init; }

        public string Password { get; init; } = string.Empty;
    }
}
