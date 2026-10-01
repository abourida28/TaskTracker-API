using TaskTracker.Models;
using TaskTracker.Repositories.EFCore;
using TaskTracker.Repositories.Interfaces;
using TaskTracker.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.");

var version = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, version));


builder.Services.Configure<ClockifySettings>(builder.Configuration.GetSection("Clockify"));

var clockifyKey = builder.Configuration["Clockify:ApiKey"] ?? builder.Configuration["Clockify:Key"];
if (string.IsNullOrEmpty(clockifyKey))
{
    throw new Exception("Clockify API key is not set in configuration.");
}

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services.AddScoped<IRemoteService, ClockifyRemoteService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
