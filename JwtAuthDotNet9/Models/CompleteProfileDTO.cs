namespace JwtAuthDotNet9.Models
{
    public class CompleteProfileDTO
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string CIN { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? AdditionalInfos { get; set; }
    }
}