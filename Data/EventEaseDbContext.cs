using EventEase_st10157545_POE.Controllers;
using EventEase_st10157545_POE.Models;
using Microsoft.EntityFrameworkCore;
namespace EventEase_st10157545_POE.Data
{
    public class EventEaseDbContext : DbContext
    {

        public EventEaseDbContext(DbContextOptions<EventEaseDbContext> options) : base(options) { }

        public DbSet<VenueViewModel> Venue { get; set; }
        public DbSet<EventViewModel> Event { get; set; }
        public DbSet<CustomerViewModel> Customer { get; set; }

        public DbSet<BookingSpecialistViewModelcs> BookingSpecialist { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<VenueAvaliabilityViewModel> VenueSchedules { get; set; }

        public DbSet<AuditLogViewModel> AuditLog { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Prevent cascade delete loops on Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Specialist)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SpecialistID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VenueAvaliabilityViewModel>()
                .HasOne(vs => vs.Booking)
                .WithOne(b => b.Schedule)
                .HasForeignKey<VenueAvaliabilityViewModel>(vs => vs.BookingID)
                .OnDelete(DeleteBehavior.SetNull);

            //Seed a default admin specialist
            modelBuilder.Entity<BookingSpecialistViewModelcs>().HasData(new BookingSpecialistViewModelcs
            {
                SpecialistID = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@eventease.co.za",
                Password = "$2b$11$zWmUQTosMUfdINFVTppjbOmy6W5kuhO9CyP6Sc4sZzCsA3trwcO0y",
                Role = "Admin"
            });

        }
    }
}
