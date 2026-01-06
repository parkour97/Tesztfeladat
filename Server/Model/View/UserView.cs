namespace Server.Model.View
{
    public class UserView
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string UsernameOrEmail, string Password);
}
