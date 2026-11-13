using Microsoft.EntityFrameworkCore;
using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Data;

public sealed class PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : DbContext(options)
{
    public DbSet<Medicine> Medicines => Set<Medicine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd();
            entity.HasKey("Id");
            entity.Property(medicine => medicine.Name).IsRequired();
            entity.Property(medicine => medicine.Brand).IsRequired();
        });
    }
}
