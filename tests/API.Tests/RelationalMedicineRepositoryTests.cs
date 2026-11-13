using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Api.Data;
using PharmacyApp.Api.DataModels;
using PharmacyApp.Api.Repositories;
using Xunit;

namespace PharmacyApp.Api.Tests;

public sealed class RelationalMedicineRepositoryTests
{
    [Fact]
    public async Task RepositorySupportsSearchPagingAndPersistingAdds()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new PharmacyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Medicines.AddRange(NewMedicine("Paracetamol 500mg"), NewMedicine("Amoxicillin 250mg"));
        await dbContext.SaveChangesAsync();

        var repository = new PostgresMedicineRepository(dbContext);
        var searchResults = (await repository.SearchAsync("PARA")).ToList();
        var secondPage = await repository.GetAllAsync(null, 2, 1);
        var added = await repository.AddAsync(NewMedicine("Vitamin C"));
        var allMedicines = await repository.GetAllAsync(null, 1, 10);

        Assert.Single(searchResults);
        Assert.Equal("Paracetamol 500mg", searchResults[0].Name);
        Assert.Equal("Amoxicillin 250mg", Assert.Single(secondPage.Items).Name);
        Assert.Equal(2, secondPage.TotalPages);
        Assert.Equal(3, allMedicines.TotalCount);
        Assert.Contains(allMedicines.Items, medicine => medicine.Name == added.Name);
    }

    [Fact]
    public async Task AddRejectsNullMedicine()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new PharmacyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new PostgresMedicineRepository(dbContext);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    private static Medicine NewMedicine(string name)
    {
        return new Medicine
        {
            Name = name,
            Brand = "Test Brand",
            Notes = "Test notes",
            ExpiryDate = new DateOnly(2099, 12, 31),
            Quantity = 10,
            Price = 10
        };
    }
}
