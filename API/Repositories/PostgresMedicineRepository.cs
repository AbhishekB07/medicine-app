using Microsoft.EntityFrameworkCore;
using PharmacyApp.Api.Data;
using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Repositories;

public sealed class PostgresMedicineRepository(PharmacyDbContext dbContext) : IMedicineRepository
{
    public async Task<PagedResult<Medicine>> GetAllAsync(string? query, int pageNumber, int pageSize)
    {
        var medicines = GetMatchingMedicines(query);
        var totalCount = await medicines.CountAsync();
        var items = await medicines
            .OrderBy(medicine => EF.Property<int>(medicine, "Id"))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Medicine>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<IEnumerable<Medicine>> SearchAsync(string query)
    {
        return await GetMatchingMedicines(query)
            .OrderBy(medicine => EF.Property<int>(medicine, "Id"))
            .ToListAsync();
    }

    public async Task<Medicine> AddAsync(Medicine medicine)
    {
        ArgumentNullException.ThrowIfNull(medicine);

        dbContext.Medicines.Add(medicine);
        await dbContext.SaveChangesAsync();
        return medicine;
    }

    private IQueryable<Medicine> GetMatchingMedicines(string? query)
    {
        var medicines = dbContext.Medicines.AsNoTracking();
        if (string.IsNullOrWhiteSpace(query))
        {
            return medicines;
        }

        var escapedQuery = EscapeLikePattern(query.Trim().ToLowerInvariant());
        return medicines.Where(medicine =>
            EF.Functions.Like(medicine.Name.ToLower(), $"%{escapedQuery}%", "\\"));
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
