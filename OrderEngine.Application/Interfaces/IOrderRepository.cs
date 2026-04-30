namespace OrderEngine.Application.Interfaces;
using OrderEngine.Domain.Entities;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
}