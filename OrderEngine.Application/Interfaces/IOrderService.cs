// Services/IOrderService.cs
using OrderEngine.Application.DTOs;
using OrderEngine.Domain.Enums;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status);
    Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto);
    Task DeleteAsync(Guid id);
}