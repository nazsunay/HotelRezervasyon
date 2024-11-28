using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reservation tablosunun birincil anahtarı
            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.Id);

        

            // Reservation -> Slot (Many-to-One)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Slot tablosu için yapılandırma
            modelBuilder.Entity<Room>()
                .Property(s => s.IsAvailable)
                .IsRequired(); // Required alan, boş geçilmemeli




            modelBuilder.Entity<Reservation>()
                .Property(r => r.CheckInDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}


