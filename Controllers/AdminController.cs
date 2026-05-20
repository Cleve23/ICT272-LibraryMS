using LibraryMS.Data;
using LibraryMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalMembers = await _context.Users.CountAsync();
            ViewBag.ActiveBorrows = await _context.BorrowRecords
                .Where(b => b.Status == BorrowStatus.Borrowed).CountAsync();
            ViewBag.OverdueBooks = await _context.BorrowRecords
                .Where(b => b.Status == BorrowStatus.Overdue).CountAsync();
            return View();
        }

        public async Task<IActionResult> LibraryProfile()
        {
            var profile = await _context.LibraryProfiles.FirstOrDefaultAsync();
            return View(profile);
        }

        public IActionResult CreateProfile() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(LibraryProfile model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.LibraryProfiles.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LibraryProfile));
        }

        public async Task<IActionResult> EditProfile(int id)
        {
            var profile = await _context.LibraryProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(LibraryProfile model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.LibraryProfiles.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LibraryProfile));
        }

        public async Task<IActionResult> Books()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        public IActionResult AddBook() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(Book model, IFormFile? coverImage)
        {
            if (!ModelState.IsValid) return View(model);

            if (coverImage != null && coverImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await coverImage.CopyToAsync(stream);
                model.CoverImagePath = "/uploads/covers/" + fileName;
            }

            model.AvailableCopies = model.TotalCopies;
            model.IsAvailable = model.TotalCopies > 0;
            _context.Books.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Books));
        }

        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Book model, IFormFile? coverImage)
        {
            if (!ModelState.IsValid) return View(model);

            if (coverImage != null && coverImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await coverImage.CopyToAsync(stream);
                model.CoverImagePath = "/uploads/covers/" + fileName;
            }

            model.IsAvailable = model.AvailableCopies > 0;
            _context.Books.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Books));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Books));
        }

        public async Task<IActionResult> Transactions()
        {
            var records = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();
            return View(records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record != null)
            {
                record.ReturnDate = DateTime.Now;
                record.Status = BorrowStatus.Returned;

                if (DateTime.Now > record.DueDate)
                {
                    var profile = await _context.LibraryProfiles.FirstOrDefaultAsync();
                    var rate = profile?.DailyFineRate ?? 0.50m;
                    var days = (DateTime.Now - record.DueDate).Days;
                    record.FineAmount = days * rate;
                }

                if (record.Book != null)
                {
                    record.Book.AvailableCopies++;
                    record.Book.IsAvailable = true;
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Transactions));
        }

        public async Task<IActionResult> Reports()
        {
            var overdueRecords = await _context.BorrowRecords
                .Where(b => b.Status == BorrowStatus.Borrowed && b.DueDate < DateTime.Now)
                .ToListAsync();
            foreach (var r in overdueRecords)
                r.Status = BorrowStatus.Overdue;
            await _context.SaveChangesAsync();

            ViewBag.MostBorrowed = await _context.BorrowRecords
                .Include(b => b.Book)
                .GroupBy(b => b.Book!.Title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.OverdueList = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.Status == BorrowStatus.Overdue)
                .ToListAsync();

            ViewBag.ActiveMembers = await _context.BorrowRecords
                .Include(b => b.User)
                .GroupBy(b => b.User!.Email)
                .Select(g => new { Email = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
