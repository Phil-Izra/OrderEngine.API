using Microsoft.EntityFrameworkCore;
using OrderEngine.Application;
using OrderEngine.Application.Interfaces;
using OrderEngine.Application.Services;
using OrderEngine.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() {
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();