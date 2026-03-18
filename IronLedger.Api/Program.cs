using IronLedger.Api.Endpoints;
using IronLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
var connectionString = File.ReadAllLines(envPath)
    .First(l => l.StartsWith("ConnectionStrings__DefaultConnection="))
    .Split('=', 2)[1];

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OnyxDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OnyxDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IronLedger API v1");
    });
}

PaymentEndpoints.Map(app);

app.Run();
