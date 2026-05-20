// OrderEngine.API/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderEngine.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;
    [HttpGet("stats")]
    [AllowAnonymous]
    // Public - no auth needed
    public async Task<IActionResult> GetStats()
    {
        var orders = await _db.Orders
        .Include(o => o.Items)
        .ToListAsync();

        var stats = new
        {
            totalOrders = orders.Count,
            totalRevenue = orders.Sum(o => o.FinalAmount),
            totalDiscount = orders.Sum(o => o.DiscountAmount),
            averageOrderValue = orders.Any()
        ? orders.Average(o => o.FinalAmount) : 0,
            // Orders per day for bar chart (last 7 days)
            ordersPerDay = orders
        .Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-7))
        .GroupBy(o => o.CreatedAt.Date)
        .Select(g => new
        {
            date = g.Key.ToString("MMM dd"),
            count = g.Count(),
            revenue = g.Sum(o => o.FinalAmount)
        })
        .OrderBy(x => x.date)
        .ToList(),
            // Discount breakdown for pie chart
            discountBreakdown = new[]
        {
new { name = "No Discount",
value = orders.Count(o => o.DiscountAmount == 0) },
new { name = "Percentage",
value = orders.Count(o =>
o.DiscountAmount > 0 && o.DiscountAmount < 100) },
new { name = "Bulk Order",
value = orders.Count(o => o.DiscountAmount >= 100) },
}
        };
        return Ok(stats);
    }
}