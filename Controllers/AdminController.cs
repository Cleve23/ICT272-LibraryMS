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

        // Dashboard
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

        // ── Library Profile ──────────────────────────────────────────

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
    }
}