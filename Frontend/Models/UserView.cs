using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class UserView
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Username or email is required")]
        public string UsernameOrEmail { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
