namespace OrderEngine.Application.Interfaces;
using OrderEngine.Domain.Entities;

public interface IDiscountStrategy
{
    decimal ApplyDiscount(Order order);
    bool IsApplicable(Order order);
}