using Microsoft.EntityFrameworkCore;
using EAFC27.Infrastructure.Data;
using EAFC27.Application.Interfaces;
using EAFC27.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICardSeederService, CardSeederService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<ICardSeederService>();
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "cards.csv");
    
    await seeder.SeedCardsAsync(csvPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();