using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Api.Database;
using PharmacyApp.Api.Data;
using PharmacyApp.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Json";
if (databaseProvider.Equals("Json", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
}
else if (databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("PharmacyDatabase")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:PharmacyDatabase must be configured when Database:Provider is PostgreSql.");

    builder.Services.AddDbContext<PharmacyDbContext>(options => options.UseNpgsql(connectionString));
    builder.Services.AddScoped<IMedicineRepository, PostgresMedicineRepository>();
}
else
{
    throw new InvalidOperationException(
        $"Unsupported database provider '{databaseProvider}'. Supported providers are 'Json' and 'PostgreSql'.");
}

var app = builder.Build();

if (databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.Medicines.AnyAsync())
    {
        dbContext.Medicines.AddRange(MedicineSeedData.Create());
        await dbContext.SaveChangesAsync();
    }
}

app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();

app.UseExceptionHandler("/error");
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
