using EventEase_st10157545_POE.Data;
using Microsoft.AspNetCore.Mvc;
using EventEase_st10157545_POE.Services;
using EventEase_st10157545_POE.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models;
using EventEase_st10157545_POE.Filter;

namespace EventEase_st10157545_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly EventEaseDbContext _context;
        private readonly AuthService _auth;

        public AccountController(EventEaseDbContext context, AuthService auth) {
            _context = context;
            _auth = auth;
        }

        // GET: /Account/Login 
        public IActionResult Login(string? returnUrl)
        {
            if (_auth.IsSignedIn())
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        //  POST: /Account/Login 
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var specialist = await _context.BookingSpecialist
                .FirstAsync(s => s.Email == vm.Email);
            if (specialist == null || !BCrypt.Net.BCrypt.Verify(vm.Password, specialist.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }
            _auth.SignIn(specialist);
            TempData["Success"] = $"Welcome back, {specialist.FirstName}!";
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }
        //  POST: /Account/Logout 
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _auth.SignOut();
            TempData["Success"] = "You have been signed out.";
            return RedirectToAction(nameof(Login));
        }
        //  GET: /Account/Register  (Admin only) 
        [RequireAdmin]
        public IActionResult Register()
            => View(new RegisterViewModel());
        //  POST: /Account/Register 
        [HttpPost, ValidateAntiForgeryToken]
        [RequireAdmin]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (await _context.BookingSpecialist.AnyAsync(s => s.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(vm);
            }
            var specialist = new BookingSpecialistViewModelcs
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Role = vm.Role
            };
            _context.BookingSpecialist.Add(specialist);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Account created for {specialist.FullName} ({specialist.Role}).";
            return RedirectToAction("Index", "Specialists");
        }
        //  GET: /Account/Profile
        [RequireLogin]
        public async Task<IActionResult> Profile()
        {
            var id = _auth.GetCurrentId();
            var specialist = await _context.BookingSpecialist.FindAsync(id);
            if (specialist == null) return RedirectToAction(nameof(Login));
            ViewData["ChangePasswordVm"] = new ChangePasswordViewModel();
            return View(specialist);
        }
        // POST: /Account/ChangePassword 
        [HttpPost, ValidateAntiForgeryToken]
        [RequireLogin]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            var id = _auth.GetCurrentId();
            var specialist = await _context.BookingSpecialist.FindAsync(id);
            if (specialist == null) return RedirectToAction(nameof(Login));
            if (!ModelState.IsValid)
            {
                ViewData["ChangePasswordVm"] = vm;
                return View("Profile", specialist);
            }
            if (!BCrypt.Net.BCrypt.Verify(vm.CurrentPassword, specialist.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                ViewData["ChangePasswordVm"] = vm;
                return View("Profile", specialist);
            }
            specialist.Password = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }
        //  GET: /Account/AccessDenied 
        public IActionResult AccessDenied()
            => View();
    }
}
