using EventEase_st10157545_POE.Data;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Services;
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
            builder.Services.AddHttpContextAccessor();


            // Session (used to hold the logged-in specialist's ID
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => { 
                options.IdleTimeout = TimeSpan.FromSeconds(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            
            });


            // Add services to the container.
            builder.Services.AddControllersWithViews();

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

            app.UseAuthorization();

            app.UseSession();//enable session middleware
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
