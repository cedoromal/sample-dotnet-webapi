using Microsoft.EntityFrameworkCore;
using Minio;
using sample_dotnet_webapi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseMySql(
        builder.Configuration["Database:ConnectionString"],
        new MySqlServerVersion(new Version(8, 4, 3))
    )
);

builder.Services.AddMinio(configureClient =>
    configureClient
        .WithEndpoint(builder.Configuration["S3:Endpoint"])
        .WithCredentials(
            builder.Configuration["S3:AccessKey"],
            builder.Configuration["S3:SecretKey"]
        )
        .WithSSL()
        .Build()
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

    // Configure the HTTP request pipeline and create database.
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        dbContext.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
