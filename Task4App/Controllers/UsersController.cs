using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4App.Data;
using Task4App.Models;

namespace Task4App.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<User?> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        }

        private async Task<IActionResult?> EnsureAuthenticatedAndActive()
        {
            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.IsBlocked)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var guard = await EnsureAuthenticatedAndActive();
            if (guard != null) return guard;

            var currentUser = await GetCurrentUser();
            var users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

            foreach (var user in users)
            {
                user.IsCurrentUser = currentUser != null && user.Id == currentUser.Id;
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Block(List<int> selectedUserIds)
        {
            var guard = await EnsureAuthenticatedAndActive();
            if (guard != null) return guard;

            var currentUser = await GetCurrentUser();

            if (selectedUserIds != null && selectedUserIds.Any())
            {
                var users = await _context.Users.Where(u => selectedUserIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    user.IsBlocked = true;
                }

                await _context.SaveChangesAsync();

                if (currentUser != null && selectedUserIds.Contains(currentUser.Id))
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(List<int> selectedUserIds)
        {
            var guard = await EnsureAuthenticatedAndActive();
            if (guard != null) return guard;

            if (selectedUserIds != null && selectedUserIds.Any())
            {
                var users = await _context.Users.Where(u => selectedUserIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    user.IsBlocked = false;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}