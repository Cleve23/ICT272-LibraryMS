using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Models
{
    public class BookFeedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(1000)]
        [Display(Name = "Comment")]
        public string? Comment { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser? User { get; set; }
        public Book? Book { get; set; }
    }
}