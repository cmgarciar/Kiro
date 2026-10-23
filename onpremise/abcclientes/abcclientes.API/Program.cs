using abcclientes.API.Middleware;
using abcclientes.Application.Interfaces;
using abcclientes.Application.Services;
using abcclientes.Domain.Interfaces;
using abcclientes.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register connection string in configuration
builder.Configuration["ConnectionStrings:DefaultConnection"] =
    builder.Configuration.GetConnectionString("DefaultConnection");

// Application services
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Infrastructure repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for WinForms client
builder.Services.AddCors(options =>
{
    options.AddPolicy("WinFormsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Global exception handler
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("WinFormsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
