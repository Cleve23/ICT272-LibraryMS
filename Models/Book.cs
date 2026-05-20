using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Author")]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Genre")]
        public string Genre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Cover Image")]
        public string? CoverImagePath { get; set; }

        [StringLength(1000)]
        [Display(Name = "Summary")]
        public string? Summary { get; set; }

        [Display(Name = "Total Copies")]
        [Range(1, 100)]
        public int TotalCopies { get; set; } = 1;

        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; } = 1;

        // Navigation
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
        public ICollection<BookFeedback> Feedbacks { get; set; } = new List<BookFeedback>();
    }
}