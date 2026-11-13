# Sapient Pharmacy App

A pharmacy application with an ASP.NET Core Web API and Angular UI.

## Run the API

```bash
cd API
dotnet run --urls http://localhost:5010
```

By default, the API stores medicine data in `API/Database/medicines.json`.

## Database providers

The API uses the JSON repository by default. Select PostgreSQL by setting
`Database:Provider` to `PostgreSql` in `API/appsettings.json`, then provide the
connection string with the `ConnectionStrings__PharmacyDatabase` environment
variable:

```bash
cd API
Database__Provider=PostgreSql \
ConnectionStrings__PharmacyDatabase='Host=localhost;Port=5432;Database=pharmacy;Username=postgres;Password=your-password' \
dotnet run --urls http://localhost:5010
```

The PostgreSQL database is initialized and seeded on first startup. Changing
the provider does not migrate or synchronize data between JSON and PostgreSQL;
each provider stores its data independently.

## Run the UI

```bash
cd UI
npm install
ng serve
```

The UI runs at `http://localhost:4200`.

## Tests

```bash
dotnet test API.Tests/PharmacyApp.Api.Tests.csproj
```

## Build

```bash
cd API
dotnet build

cd ../UI
ng build
```
