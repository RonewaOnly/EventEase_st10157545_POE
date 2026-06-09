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

        public DbSet<VenueAvaliabilityViewModel> VenueSchedules { get; set; } // Add DbSet for VenueAvaliabilityViewModel

        public DbSet<AuditLogViewModel> AuditLog { get; set; } // Add DbSet for AuditLogViewModel

        public DbSet<EventType> EventTypes { get; set; }  // Add DbSet for EventType



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

            // Seed: EventType lookup table
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeID = 1, TypeName = "Wedding", Description = "Wedding ceremonies and receptions", IsActive = true },
                new EventType { EventTypeID = 2, TypeName = "Corporate Conference", Description = "Business meetings, conferences and seminars", IsActive = true },
                new EventType { EventTypeID = 3, TypeName = "Birthday Party", Description = "Birthday celebrations of all ages", IsActive = true },
                new EventType { EventTypeID = 4, TypeName = "Product Launch", Description = "Product reveals and brand launch events", IsActive = true },
                new EventType { EventTypeID = 5, TypeName = "Gala Dinner", Description = "Formal dinner and awards ceremonies", IsActive = true },
                new EventType { EventTypeID = 6, TypeName = "Workshop / Training", Description = "Educational sessions and skills workshops", IsActive = true },
                new EventType { EventTypeID = 7, TypeName = "Exhibition", Description = "Trade shows and public exhibitions", IsActive = true },
                new EventType { EventTypeID = 8, TypeName = "Concert / Show", Description = "Live music, theatre and entertainment shows", IsActive = true },
                new EventType { EventTypeID = 9, TypeName = "Sports Event", Description = "Sporting competitions and tournaments", IsActive = true },
                new EventType { EventTypeID = 10, TypeName = "Private Function", Description = "Private gatherings and family functions", IsActive = true }
            );

        }
    }
}
