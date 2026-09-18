using Convocado.Api.Features.Health;
using Convocado.Api.Infrastructure.Http;
using Convocado.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Convocado")
    ?? throw new InvalidOperationException("A connection string 'Convocado' não foi configurada.");
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
builder.Services.AddDbContext<QueryDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
builder.Services.AddDbContext<CommandDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll));
builder.Services.AddScoped<GetHealth.Handler>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGetHealth();

app.Run();
