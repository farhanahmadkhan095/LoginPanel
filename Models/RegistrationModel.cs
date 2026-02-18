using System.ComponentModel.DataAnnotations;

namespace LoginPanel.Models
{
    public class RegistrationModel
    {
        [Required (ErrorMessage="UserName is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password minimum 8 characters ka hona chahiye.")]
        public string Password { get; set; }
        [MinLength(8, ErrorMessage = "ConfirmPassword minimum 8 characters ka hona chahiye.")]
        [Compare("Password", ErrorMessage = "Password match nahi ho raha!")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Email is required.")]

        public string Email { get; set; }
        [Required(ErrorMessage = "MobileNo is required.")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "CountryId is required.")]
        public int CountryId { get; set; }
        [Required(ErrorMessage = "StateId is required.")]
        public int StateId { get; set; }
        [Required(ErrorMessage = "CityId is required.")]
        public int CityId { get; set; }
    }
}

