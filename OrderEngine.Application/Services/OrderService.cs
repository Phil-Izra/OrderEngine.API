using OrderEngine.Application.DTOs;
using OrderEngine.Application.Interfaces;
using OrderEngine.Domain.Entities;

public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public OrderService(IOrderRepository repo,
        IEnumerable<IDiscountStrategy> strategies)
    {
        _repo = repo;
        _strategies = strategies;
    }

    public async Task<Order> ProcessOrderAsync(CreateOrderDto dto)
    {
        var order = MapToOrder(dto);
        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        // Apply best applicable discount
        var bestDiscount = _strategies
            .Where(s => s.IsApplicable(order))
            .Select(s => s.ApplyDiscount(order))
            .DefaultIfEmpty(0)
            .Max();

        order.DiscountAmount = bestDiscount;
        order.FinalAmount = order.TotalAmount - bestDiscount;

        return await _repo.CreateAsync(order);
    }
    private static OrderResponseDto MapToResponse(Order order) => new()
{
    Id = order.Id,
    CustomerId = order.CustomerId,
    CustomerName = order.Customer.Name,
    TotalAmount = order.TotalAmount,
    DiscountAmount = order.DiscountAmount,
    FinalAmount = order.FinalAmount,
    CreatedAt = order.CreatedAt,
    Items = order.Items.Select(i => new OrderItemDto
    {
        Id = i.Id,
        ProductName = i.ProductName,
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice,
        TotalPrice = i.TotalPrice
    }).ToList()
};

    private static Order MapToOrder(CreateOrderDto dto) => new()
    {
        CustomerId = dto.CustomerId,
        Items = dto.Items.Select(i => new OrderItem
        {
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}