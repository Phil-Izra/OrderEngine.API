using OrderEngine.Domain.Enums;

namespace OrderEngine.Application.DTOs;

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
public class CreateOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateStatusDto
{
    public OrderStatus Status { get; set; }
}