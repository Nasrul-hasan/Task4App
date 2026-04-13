using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4App.Data;
using Task4App.Helpers;
using Task4App.Models;

namespace Task4App.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedEmail = model.Email.Trim().ToLower();

            var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = normalizedEmail,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                IsConfirmed = false,
                IsBlocked = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirm", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult Confirm(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login");

            ViewBag.Email = email.Trim().ToLower();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPost(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login");

            var normalizedEmail = email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
                return RedirectToAction("Login");

            if (user.IsConfirmed)
            {
                TempData["SuccessMessage"] = $"User {normalizedEmail} is already confirmed.";
                return RedirectToAction("Login");
            }

            user.IsConfirmed = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User {normalizedEmail} confirmed successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedEmail = model.Email.Trim().ToLower();
            var passwordHash = PasswordHelper.HashPassword(model.Password);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (user.PasswordHash != passwordHash)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (!user.IsConfirmed)
            {
                ModelState.AddModelError("", "User is not confirmed.");
                return View(model);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "User is blocked.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);

            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}