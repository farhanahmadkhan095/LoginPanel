using System.ComponentModel.DataAnnotations;

namespace LoginPanel.Models
{
    public class CheckoutAddressVM
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string AddressLine { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Pincode { get; set; } = string.Empty;
    }
}


//using System.ComponentModel.DataAnnotations;

//namespace LoginPanel.Models
//{
//    public class CheckoutAddressVM
//    {
//        [Required, StringLength(60)]
//        public string FullName { get; set; }

//        [Required, StringLength(15)]
//        public string Phone { get; set; }

//        [Required, StringLength(200)]
//        public string AddressLine1 { get; set; }

//        [StringLength(200)]
//        public string? AddressLine2 { get; set; }

//        [Required, StringLength(60)]
//        public string City { get; set; }

//        [Required, StringLength(60)]
//        public string State { get; set; }

//        [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Enter valid 6-digit pincode")]
//        public string Pincode { get; set; }

//        [StringLength(80)]
//        public string? Landmark { get; set; }
//    }
//}
