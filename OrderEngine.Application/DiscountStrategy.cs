using OrderEngine.Application.Interfaces;
using OrderEngine.Domain.Entities;

namespace OrderEngine.Application;

// PercentageDiscountStrategy.cs
public class PercentageDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _percentage;
    public PercentageDiscountStrategy(decimal percentage) => _percentage = percentage;

    public bool IsApplicable(Order order) => order.TotalAmount > 0;
    public decimal ApplyDiscount(Order order) =>
        Math.Round(order.TotalAmount * (_percentage / 100), 2);

}

// BulkOrderDiscountStrategy.cs
public class BulkOrderDiscountStrategy : IDiscountStrategy
{
    public bool IsApplicable(Order order) => order.Items.Sum(i => i.Quantity) >= 10;
    public decimal ApplyDiscount(Order order) =>
        Math.Round(order.TotalAmount * 0.15m, 2); // 15% for bulk
}

// PremiumCustomerDiscountStrategy.cs
public class PremiumCustomerDiscountStrategy : IDiscountStrategy
{
    public bool IsApplicable(Order order) => order.Customer.IsPremium;
    public decimal ApplyDiscount(Order order) =>
        Math.Round(order.TotalAmount * 0.20m, 2); // 20% for premium
}