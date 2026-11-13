using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Database;

public static class MedicineSeedData
{
    public static IReadOnlyList<Medicine> Create() =>
    [
        new Medicine
        {
            Name = "Paracetamol 500mg",
            Notes = "Used for pain relief and fever",
            ExpiryDate = new DateOnly(2028, 12, 31),
            Quantity = 120,
            Price = 25.50m,
            Brand = "Generic"
        },
        new Medicine
        {
            Name = "Amoxicillin 250mg",
            Notes = "Prescription antibiotic",
            ExpiryDate = new DateOnly(2027, 6, 30),
            Quantity = 80,
            Price = 140.00m,
            Brand = "MediCare"
        },
        new Medicine
        {
            Name = "Vitamin C 1000mg",
            Notes = "Immune support supplement",
            ExpiryDate = new DateOnly(2029, 3, 15),
            Quantity = 200,
            Price = 65.75m,
            Brand = "HealthPlus"
        }
    ];
}
