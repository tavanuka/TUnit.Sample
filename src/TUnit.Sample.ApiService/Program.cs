using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TUnit.Sample.ApiService.Endpoints.Books;
using TUnit.Sample.ApiService.Endpoints.Persons;
using TUnit.Sample.ApiService.Endpoints.WeatherForecasts;
using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Common.Constants;
using TUnit.Sample.Infrastructure;
using TUnit.Sample.Infrastructure.Data.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CoreDbContext>(ResourceConstants.CoreDb, configureDbContextOptions: op =>
    op.UseAsyncSeeding(async (context, _, ct)
        => await CoreDbContext.SeedDomainObjects(context, ct)
    )
);

// Add services to the container.
builder.Services.TryAddScoped<IPersonService, PersonService>();
builder.Services.TryAddSingleton<IAgeCalculator, AgeCalculator>();
builder.Services.TryAddSingleton<IWeatherForecastService, WeatherForecastService>();

builder.Services.AddSingleton<BookService>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// this should not be used for production applications.
// await using (var serviceScope = app.Services.CreateAsyncScope())
// await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<CoreDbContext>())
// {
//     var strategy = dbContext.Database.CreateExecutionStrategy();
//     await strategy.ExecuteAsync(
//         dbContext,
//         async static (db, ct) => await db.Database.EnsureCreatedAsync(ct),
//         CancellationToken.None
//     );
// }

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");
app.MapWeatherForecastEndpoints();
app.MapPersonsEndpoints();
app.MapBooksEndpoints();

app.MapDefaultEndpoints();

app.Run();