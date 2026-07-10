using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StockApp.Data;
using StockApp.Models.Entities;
using StockApp.Repositories;
using StockApp.Services;
using StockApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("JWT_KEY is not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// CORS — allow Vue dev server + Render
var corsOrigins = new[] { "http://localhost:5173" }
    .Concat(
        Environment.GetEnvironmentVariable("CORS_ORIGINS")?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
        ?? []
    )
    .ToArray();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// LiteDB — singleton (path from env or default)
var dbPath = Environment.GetEnvironmentVariable("LITEDB_PATH")
    ?? Path.Combine(AppContext.BaseDirectory, "stock.db");
var dbDir = Path.GetDirectoryName(dbPath);
if (dbDir is not null && !Directory.Exists(dbDir))
    Directory.CreateDirectory(dbDir);
builder.Services.AddSingleton(new LiteDbContext(dbPath));

// Repositories
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<MovementRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<WithdrawDetailRepository>();
builder.Services.AddScoped<CompanyRepository>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CompanyContext>();

var app = builder.Build();

// Seed default user
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<LiteDbContext>();
    var userRepo = scope.ServiceProvider.GetRequiredService<UserRepository>();
    var admin = userRepo.GetByUsername("admin");
    if (admin == null)
    {
        userRepo.Insert(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
        });
    }
    else
    {
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        userRepo.Update(admin);
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Serve Vue frontend (static files)
app.UseDefaultFiles();
app.UseStaticFiles();

// SPA fallback — return index.html for non-API routes
app.MapFallbackToFile("index.html");

app.Run();
