using Concessionaire.WebAPI.Repositories;
using Concessionaire.WebAPI.Services;
using System.Data;
using System.Data.SqlClient; // Necesario para IDbConnection

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Conexión a la base de datos SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IDbConnection>((sp) => new SqlConnection(connectionString));

builder.Services.AddScoped<ICarsRepository, CarsRepository>();
builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
