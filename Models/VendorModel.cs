namespace LoginPanel.Models
{
    public class VendorModel
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
