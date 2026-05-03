using Microsoft.EntityFrameworkCore;
using OrderEngine.Application;
using OrderEngine.Application.Interfaces;
using OrderEngine.Application.Services;
using OrderEngine.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add after AddControllers()
var jwt = builder.Configuration.GetSection("JwtSettings");
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
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
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

var app = builder.Build();

// Static files for photo serving
app.UseStaticFiles();

app.UseCors("AllowReact");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();