using OnPremise.API.Middleware;
using OnPremise.Application.Interfaces;
using OnPremise.Application.Services;
using OnPremise.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "WinFormsClient";

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (repositorios ADO.NET) y casos de uso.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IClienteService, ClienteService>();

// CORS para el cliente WinForms local.
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();
