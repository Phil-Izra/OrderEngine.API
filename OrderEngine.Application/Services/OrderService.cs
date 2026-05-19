using Microsoft.Extensions.Logging;
using OrderEngine.Application.DTOs;
using OrderEngine.Application.Exceptions;
using OrderEngine.Application.Interfaces;
using OrderEngine.Domain.Entities;
using OrderEngine.Domain.Enums;

namespace OrderEngine.Application.Services;

public class OrderService(
    IOrderRepository repository,
    ILogger<OrderService> logger) : IOrderService
{

    public async Task<OrderDto> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Fetching order {OrderId}", id);

        var order = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await repository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        logger.LogInformation("Creating order for customer {CustomerId}", dto.CustomerId);

        var totalAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
        var order = new Order
        {
            CustomerId = dto.CustomerId,
            Items = dto.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalAmount = totalAmount,
            DiscountAmount = 0,
            FinalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order = await repository.CreateAsync(order);

        logger.LogInformation("Order {OrderId} created, total {TotalAmount}",
            order.Id, order.TotalAmount);

        return MapToDto(order);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var order = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        logger.LogInformation("Order {OrderId} status: {OldStatus} → {NewStatus}",
            id, order.Status, status);

        order.Status = status;

        await repository.UpdateAsync(order);
        return MapToDto(order);
    }

    public async Task DeleteAsync(Guid id)
    {
        logger.LogInformation("Deleting order {OrderId}", id);
        await repository.DeleteAsync(id);
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        CustomerName = order.Customer?.Name ?? string.Empty,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice
        }).ToList(),
        TotalAmount = order.TotalAmount,
        DiscountAmount = order.DiscountAmount,
        FinalAmount = order.FinalAmount,
        CreatedAt = order.CreatedAt,
        Status = order.Status
    };
}