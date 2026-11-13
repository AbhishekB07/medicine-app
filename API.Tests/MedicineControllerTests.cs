using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Api.Contracts.Medicines;
using PharmacyApp.Api.Controllers;
using PharmacyApp.Api.DataModels;
using PharmacyApp.Api.Repositories;
using Xunit;

namespace PharmacyApp.Api.Tests;

public sealed class MedicineControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResponse()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        var result = await controller.GetAllAsync();

        var response = Assert.IsType<MedicineListResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(1, response.PageNumber);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetAll_PassesQueryAndPaginationToRepository()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        await controller.GetAllAsync("para", 2, 5);

        Assert.Equal(("para", 2, 5), repository.LastGetAll);
    }

    [Fact]
    public async Task GetAll_RejectsPageNumberBelowOne()
    {
        var controller = new MedicineController(new FakeMedicineRepository());

        var result = await controller.GetAllAsync(pageNumber: 0);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_RejectsPageSizeAboveOneHundred()
    {
        var controller = new MedicineController(new FakeMedicineRepository());

        var result = await controller.GetAllAsync(pageSize: 101);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Search_ReturnsMatchingResponseItems()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        var result = await controller.SearchAsync("para");

        var response = Assert.IsAssignableFrom<IEnumerable<MedicineResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value).ToList();
        Assert.Single(response);
        Assert.Equal("Paracetamol", response[0].Name);
    }

    [Fact]
    public async Task Search_PassesQueryToRepository()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        await controller.SearchAsync("para");

        Assert.Equal("para", repository.LastSearch);
    }

    [Fact]
    public async Task AddMedicine_ReturnsCreatedResponse()
    {
        var controller = new MedicineController(new FakeMedicineRepository());
        var request = ValidRequest();

        var result = await controller.AddMedicineAsync(request);

        var response = Assert.IsType<MedicineResponse>(Assert.IsType<ObjectResult>(result.Result).Value);
        Assert.Equal(StatusCodes.Status201Created, Assert.IsType<ObjectResult>(result.Result).StatusCode);
        Assert.Equal(request.Name, response.Name);
    }

    [Fact]
    public async Task AddMedicine_MapsAllRequestFields()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);
        var request = ValidRequest();

        await controller.AddMedicineAsync(request);

        Assert.NotNull(repository.AddedMedicine);
        Assert.Equal(request.Name, repository.AddedMedicine.Name);
        Assert.Equal(request.Brand, repository.AddedMedicine.Brand);
        Assert.Equal(request.Notes, repository.AddedMedicine.Notes);
        Assert.Equal(request.Quantity, repository.AddedMedicine.Quantity);
        Assert.Equal(request.Price, repository.AddedMedicine.Price);
        Assert.Equal(request.ExpiryDate, repository.AddedMedicine.ExpiryDate);
    }

    [Fact]
    public async Task AddMedicine_RejectsPastExpiryDate()
    {
        var controller = new MedicineController(new FakeMedicineRepository());
        var request = InvalidExpiryRequest();

        var result = await controller.AddMedicineAsync(request);

        var problem = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result.Result).Value);
        Assert.Contains(nameof(CreateMedicineRequest.ExpiryDate), problem.Errors.Keys);
    }

    [Fact]
    public async Task AddMedicine_DoesNotWriteWhenExpiryIsInvalid()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);
        var request = InvalidExpiryRequest();

        await controller.AddMedicineAsync(request);

        Assert.Null(repository.AddedMedicine);
    }

    private static CreateMedicineRequest ValidRequest()
    {
        return new CreateMedicineRequest
        {
            Name = "Paracetamol",
            Brand = "Generic",
            Notes = "Pain relief",
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            Quantity = 20,
            Price = 25.50m
        };
    }

    private static CreateMedicineRequest InvalidExpiryRequest()
    {
        return new CreateMedicineRequest
        {
            Name = "Paracetamol",
            Brand = "Generic",
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today),
            Quantity = 20,
            Price = 25.50m
        };
    }

    private sealed class FakeMedicineRepository : IMedicineRepository
    {
        private readonly Medicine _medicine = new()
        {
            Name = "Paracetamol",
            Brand = "Generic",
            ExpiryDate = new DateOnly(2099, 12, 31),
            Quantity = 20,
            Price = 25
        };

        public (string? Query, int PageNumber, int PageSize)? LastGetAll { get; private set; }
        public string? LastSearch { get; private set; }
        public Medicine? AddedMedicine { get; private set; }

        public Task<PagedResult<Medicine>> GetAllAsync(string? query, int pageNumber, int pageSize)
        {
            LastGetAll = (query, pageNumber, pageSize);
            return Task.FromResult(new PagedResult<Medicine>
            {
                Items = [_medicine],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 1,
                TotalPages = 1
            });
        }

        public Task<IEnumerable<Medicine>> SearchAsync(string query)
        {
            LastSearch = query;
            return Task.FromResult<IEnumerable<Medicine>>([_medicine]);
        }

        public Task<Medicine> AddAsync(Medicine medicine)
        {
            AddedMedicine = medicine;
            return Task.FromResult(medicine);
        }
    }
}
