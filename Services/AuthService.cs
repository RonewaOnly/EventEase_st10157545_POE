using EventEase_st10157545_POE.Models;
namespace EventEase_st10157545_POE.Services
{
    public class AuthService
    {
        private const string KeyId = "Auth_SpecialistID";
        private const string KeyName = "Auth_FullName";
        private const string KeyEmail = "Auth_Email";
        private const string KeyRole = "Auth_Role";
        private readonly IHttpContextAccessor _http;
        public AuthService(IHttpContextAccessor http)
        {
            _http = http;
        }
        // ── Sign in ──────────────────────────────────────────────────────────
        public void SignIn(BookingSpecialistViewModelcs specialist)
        {
            var session = _http.HttpContext!.Session;
            session.SetInt32(KeyId, specialist.SpecialistID);
            session.SetString(KeyName, specialist.FullName);
            session.SetString(KeyEmail, specialist.Email);
            session.SetString(KeyRole, specialist.Role);
        }
        // ── Sign out ─────────────────────────────────────────────────────────
        public void SignOut()
        {
            _http.HttpContext!.Session.Clear();
        }
        // ── Read current user ────────────────────────────────────────────────
        public bool IsSignedIn()
            => _http.HttpContext?.Session.GetInt32(KeyId) != null;
        public int? GetCurrentId()
            => _http.HttpContext?.Session.GetInt32(KeyId);
        public string GetCurrentName()
            => _http.HttpContext?.Session.GetString(KeyName) ?? "";
        public string GetCurrentEmail()
            => _http.HttpContext?.Session.GetString(KeyEmail) ?? "";
        public string GetCurrentRole()
            => _http.HttpContext?.Session.GetString(KeyRole) ?? "";
        public bool IsAdmin()
            => GetCurrentRole() == "Admin";
        public bool IsSpecialist()
            => IsSignedIn() && !IsAdmin();
    }
}
