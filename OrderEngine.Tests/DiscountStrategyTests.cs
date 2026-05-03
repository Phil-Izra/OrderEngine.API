using Xunit;
using FluentAssertions;
using OrderEngine.Application;
using OrderEngine.Domain.Entities;

public class DiscountStrategyTests
{
    private Order CreateOrder(decimal total, int qty, bool isPremium = false)
    {
        return new Order
        {
            TotalAmount = total,
            Customer = new Customer { IsPremium = isPremium },
            Items = new List<OrderItem>
            {
                new() { Quantity = qty, UnitPrice = total / qty }
            }
        };
    }

    [Fact]
    public void PercentageDiscount_ShouldApply10Percent()
    {
        var strategy = new PercentageDiscountStrategy(10);
        var order = CreateOrder(200, 2);

        var discount = strategy.ApplyDiscount(order);

        discount.Should().Be(20); // 10% of 200
    }

    [Fact]
    public void BulkDiscount_ShouldApply_WhenQtyOver10()
    {
        var strategy = new BulkOrderDiscountStrategy();
        var order = CreateOrder(500, 10);

        strategy.IsApplicable(order).Should().BeTrue();
        strategy.ApplyDiscount(order).Should().Be(75); // 15% of 500
    }

    [Fact]
    public void PremiumDiscount_ShouldNotApply_ForNonPremiumCustomer()
    {
        var strategy = new PremiumCustomerDiscountStrategy();
        var order = CreateOrder(300, 3, isPremium: false);

        strategy.IsApplicable(order).Should().BeFalse();
    }

    [Fact]
    public void PremiumDiscount_ShouldApply20Percent_ForPremiumCustomer()
    {
        var strategy = new PremiumCustomerDiscountStrategy();
        var order = CreateOrder(400, 4, isPremium: true);

        strategy.IsApplicable(order).Should().BeTrue();
        strategy.ApplyDiscount(order).Should().Be(80); // 20% of 400
    }
}