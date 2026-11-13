# Sapient Pharmacy App

A pharmacy application with an ASP.NET Core Web API and Angular UI.

## Project layout

```text
src/
  API/
  UI/
tests/
  API.Tests/
deploy/
  Dockerfile
  buildspec.yml
  task-definition-template.json
```

## Run the API

```bash
cd src/API
dotnet run --urls http://localhost:5010
```

By default, the API stores medicine data in `src/API/Database/medicines.json`.

### Database providers

The API uses the JSON repository by default. Select PostgreSQL by setting
`Database:Provider` to `PostgreSql` in `src/API/appsettings.json`, then provide
the connection string with the `ConnectionStrings__PharmacyDatabase` environment
variable:

```bash
cd src/API
Database__Provider=PostgreSql \
ConnectionStrings__PharmacyDatabase='Host=localhost;Port=5432;Database=pharmacy;Username=postgres;Password=your-password' \
dotnet run --urls http://localhost:5010
```

The PostgreSQL database is initialized and seeded on first startup. Changing
the provider does not migrate or synchronize data between JSON and PostgreSQL;
each provider stores its data independently.

## Run the UI

```bash
cd src/UI
npm install
ng serve
```

The UI runs at `http://localhost:4200`.

## Tests and builds

```bash
dotnet test tests/API.Tests/API.Tests.csproj
dotnet build src/API/API.csproj
cd src/UI && npm install && npm run build
```

## Container and AWS deployment

Build the API image from the repository root with:

```bash
docker build -f deploy/Dockerfile -t sapient-task-api .
```

`deploy/buildspec.yml` builds and pushes the image to ECR and produces task
definition artifacts. Configure the CodeBuild environment with
`AWS_ACCOUNT_ID`, `IMAGE_REPO_NAME`, `ECS_EXECUTION_ROLE_ARN`,
`ECS_TASK_ROLE_ARN`, and `POSTGRES_CONNECTION_SECRET_ARN`. Store the PostgreSQL
connection string in AWS Secrets Manager and provide its ARN through
`POSTGRES_CONNECTION_SECRET_ARN`; do not put credentials in the task definition.
Enable privileged mode for the CodeBuild project. Its role needs permission to
authenticate to and push images to ECR. The ECS execution role needs permission
to pull the image, write container logs, and read the connection-string secret.
