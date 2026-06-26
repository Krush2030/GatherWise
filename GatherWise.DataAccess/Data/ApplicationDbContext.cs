using Microsoft.EntityFrameworkCore;
using GatherWise.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GatherWise.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Database Tables Mapping
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; } // <-- ADD THIS LINE
        public DbSet<VendorAssignment> VendorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Slot -> Venue Relationship
            modelBuilder.Entity<Slot>()
                .HasOne(s => s.Venue)
                .WithMany()
                .HasForeignKey(s => s.VenueId)
                .OnDelete(DeleteBehavior.Cascade); // If a venue is deleted, its calendar slots are deleted

            // 2. Booking -> Venue Relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany()
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents deleting a venue if historic bookings exist

            // 3. Booking -> Slot Relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Slot)
                .WithMany()
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents deleting a slot if it has an active booking

            // 4. Payment -> Booking Relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. VendorAssignment Many-to-Many Composite Key configuration
            modelBuilder.Entity<VendorAssignment>()
                .HasOne(va => va.Booking)
                .WithMany()
                .HasForeignKey(va => va.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VendorAssignment>()
                .HasOne(va => va.Vendor)
                .WithMany()
                .HasForeignKey(va => va.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Explicit Decimal Precision Configurations
            modelBuilder.Entity<Venue>()
                .Property(v => v.PricePerSlot)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vendor>()
                .Property(v => v.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VendorAssignment>()
                .Property(va => va.FinalAgreedPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VenueImage>()
                .HasOne(vi => vi.Venue)
                .WithMany(v => v.Images)
                .HasForeignKey(vi => vi.VenueId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}