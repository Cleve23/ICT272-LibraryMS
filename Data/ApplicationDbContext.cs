using LibraryMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<LibraryProfile> LibraryProfiles { get; set; }
        public DbSet<BookFeedback> BookFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Book -> BorrowRecords (one to many)
            builder.Entity<BorrowRecord>()
                .HasOne(b => b.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> BorrowRecords (one to many)
            builder.Entity<BorrowRecord>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Book -> Feedback (one to many)
            builder.Entity<BookFeedback>()
                .HasOne(f => f.Book)
                .WithMany(b => b.Feedbacks)
                .HasForeignKey(f => f.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Feedback (one to many)
            builder.Entity<BookFeedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}