using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryMS.Models
{
    public enum BorrowStatus
    {
        Borrowed,
        Reserved,
        Returned,
        Overdue
    }

    public class BorrowRecord
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Status")]
        public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

        [Display(Name = "Fine Amount")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; } = 0;

        [Display(Name = "Fine Paid")]
        public bool FinePaid { get; set; } = false;

        // Navigation
        public ApplicationUser? User { get; set; }
        public Book? Book { get; set; }
    }
}