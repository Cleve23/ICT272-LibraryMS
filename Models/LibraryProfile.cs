using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Models
{
    public class LibraryProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Library Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Operating Hours")]
        public string OperatingHours { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Loan Duration (Days)")]
        [Range(1, 60)]
        public int LoanDurationDays { get; set; } = 14;

        [Display(Name = "Max Renewals")]
        [Range(0, 10)]
        public int MaxRenewals { get; set; } = 2;

        [Display(Name = "Max Borrowable Items")]
        [Range(1, 20)]
        public int MaxBorrowableItems { get; set; } = 5;

        [Display(Name = "Daily Fine Rate ($)")]
        public decimal DailyFineRate { get; set; } = 0.50m;
    }
}