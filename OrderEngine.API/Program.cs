using Microsoft.EntityFrameworkCore;
using OrderEngine.Application;
using OrderEngine.Application.Interfaces;
using OrderEngine.Application.Services;
using OrderEngine.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwt["SecretKey"];
var jwtIssuer = jwt["Issuer"];
var jwtAudience = jwt["Audience"];

if (!jwt.Exists())
{
    throw new InvalidOperationException(
        "JwtSettings section is missing. Ensure JwtSettings is present in appsettings.json, appsettings.{Environment}.json, or your environment variables.");
}

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey is missing or empty. Check appsettings.json, environment-specific appsettings, or JwtSettings__SecretKey environment variable.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JwtSettings:Issuer and JwtSettings:Audience must be configured. Check appsettings.json, environment-specific appsettings, or environment variables.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme =
    JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSecret!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order Processing API",
        Version = "v1",
        Description = "Discount Engine for E-Commerce"
    });
});

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register Discount Strategies
builder.Services.AddScoped<IDiscountStrategy, PercentageDiscountStrategy>(_ =>
    new PercentageDiscountStrategy(10));
builder.Services.AddScoped<IDiscountStrategy, BulkOrderDiscountStrategy>();
builder.Services.AddScoped<IDiscountStrategy, PremiumCustomerDiscountStrategy>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Static files for photo serving
app.UseStaticFiles();

// app.UseCors("AllowReact");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();