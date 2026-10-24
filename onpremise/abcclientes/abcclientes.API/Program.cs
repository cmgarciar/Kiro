using abcclientes.API.Middleware;
using abcclientes.Application.Interfaces;
using abcclientes.Application.Services;
using abcclientes.Domain.Interfaces;
using abcclientes.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register application services and repositories
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// MVC / API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for WinForms (localhost)
builder.Services.AddCors(o => o.AddPolicy("WinFormsPolicy", p =>
    p.SetIsOriginAllowed(origin => origin.StartsWith("http://localhost"))
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("WinFormsPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
