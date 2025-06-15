using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TinyHouse.Models;

public class ApplicationDbContext : IdentityDbContext<User> 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Reservation>()
            .HasOne(r => r.House)
            .WithMany(h => h.Rezervasyonlar)
            .HasForeignKey(r => r.HouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
            .HasOne(r => r.Kiraci)
            .WithMany(u => u.Rezervasyonlar)
            .HasForeignKey(r => r.KiraciId);
    }

    public DbSet<House> TinyHouses { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<HouseImage> HouseImages { get; set; }
}