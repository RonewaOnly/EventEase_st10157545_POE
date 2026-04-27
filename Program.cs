using EventEase_st10157545_POE.Data;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Services;
using Microsoft.AspNetCore.Http.Features;
namespace EventEase_st10157545_POE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<EventEaseDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Registering Services
            builder.Services.AddScoped<BookingConflictService>();
            builder.Services.AddScoped<AuditService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<BlobStorageService>();
            builder.Services.AddHttpContextAccessor();


            // Session (used to hold the logged-in specialist's ID
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => { 
                options.IdleTimeout = TimeSpan.FromMinutes(60); // Session timeout after 60 minutes of inactivity
                options.Cookie.HttpOnly = true; // Mitigate XSS attacks by making the cookie inaccessible to client-side scripts
                options.Cookie.IsEssential = true; // Ensure the session cookie is always sent, even if the user hasn't consented to non-essential cookies
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are only sent over HTTPS

            });


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Allow larger file uploads (for venue and event images) - max 5 MB enforced in BlobStorageService
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5 MB
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseStaticFiles(); // Enable serving static files from wwwroot
            app.UseSession();//enable session middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
