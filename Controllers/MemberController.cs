using LibraryMS.Data;
using LibraryMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Browse books with search
        public async Task<IActionResult> Browse(string? search, string? genre)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));

            if (!string.IsNullOrEmpty(genre))
                books = books.Where(b => b.Genre == genre);

            ViewBag.Search = search;
            ViewBag.Genre = genre;
            ViewBag.Genres = await _context.Books.Select(b => b.Genre).Distinct().ToListAsync();

            return View(await books.ToListAsync());
        }

        // Book detail page
        public async Task<IActionResult> Detail(int id)
        {
            var book = await _context.Books
                .Include(b => b.Feedbacks)
                .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();
            return View(book);
        }

        // Borrow a book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Books.FindAsync(bookId);

            if (book == null || book.AvailableCopies <= 0)
            {
                TempData["Error"] = "This book is not available.";
                return RedirectToAction(nameof(Detail), new { id = bookId });
            }

            // Check if user already has this book
            var existing = await _context.BorrowRecords
                .AnyAsync(b => b.UserId == user!.Id && b.BookId == bookId
                    && b.Status != BorrowStatus.Returned);

            if (existing)
            {
                TempData["Error"] = "You already have this book borrowed.";
                return RedirectToAction(nameof(Detail), new { id = bookId });
            }

            // Check borrow limit
            var profile = await _context.LibraryProfiles.FirstOrDefaultAsync();
            var maxItems = profile?.MaxBorrowableItems ?? 5;
            var loanDays = profile?.LoanDurationDays ?? 14;

            var activeBorrows = await _context.BorrowRecords
                .CountAsync(b => b.UserId == user!.Id && b.Status != BorrowStatus.Returned);

            if (activeBorrows >= maxItems)
            {
                TempData["Error"] = $"You have reached the maximum borrow limit of {maxItems} items.";
                return RedirectToAction(nameof(MyBorrows));
            }

            var record = new BorrowRecord
            {
                UserId = user!.Id,
                BookId = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(loanDays),
                Status = BorrowStatus.Borrowed
            };

            book.AvailableCopies--;
            book.IsAvailable = book.AvailableCopies > 0;

            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"You have borrowed \"{book.Title}\". Due back by {record.DueDate:dd/MM/yyyy}.";
            return RedirectToAction(nameof(MyBorrows));
        }

        // My borrows history
        public async Task<IActionResult> MyBorrows()
        {
            var user = await _userManager.GetUserAsync(User);

            // Mark overdue
            var overdue = await _context.BorrowRecords
                .Where(b => b.UserId == user!.Id
                    && b.Status == BorrowStatus.Borrowed
                    && b.DueDate < DateTime.Now)
                .ToListAsync();
            foreach (var r in overdue) r.Status = BorrowStatus.Overdue;
            await _context.SaveChangesAsync();

            var records = await _context.BorrowRecords
                .Include(b => b.Book)
                .Where(b => b.UserId == user!.Id)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return View(records);
        }

        // Submit feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int bookId, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);

            // Only allow feedback if they've borrowed the book
            var hasBorrowed = await _context.BorrowRecords
                .AnyAsync(b => b.UserId == user!.Id && b.BookId == bookId);

            if (!hasBorrowed)
            {
                TempData["Error"] = "You can only review books you have borrowed.";
                return RedirectToAction(nameof(Detail), new { id = bookId });
            }

            // Remove existing feedback if any
            var existing = await _context.BookFeedbacks
                .FirstOrDefaultAsync(f => f.UserId == user!.Id && f.BookId == bookId);
            if (existing != null) _context.BookFeedbacks.Remove(existing);

            _context.BookFeedbacks.Add(new BookFeedback
            {
                UserId = user!.Id,
                BookId = bookId,
                Rating = rating,
                Comment = comment,
                DateSubmitted = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Your review has been submitted.";
            return RedirectToAction(nameof(Detail), new { id = bookId });
        }
    }
}