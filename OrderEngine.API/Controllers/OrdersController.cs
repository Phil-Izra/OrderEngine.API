using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderEngine.Application.DTOs;
using OrderEngine.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(IOrderService orderService,
            ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        { ["UserId"] = dto.CustomerId }))
        {
            var order = await orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetOrder),
                new { id = order.Id }, order);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderDto dto)
    {
        var order = await orderService.UpdateOrderAsync(id, dto);
        return Ok(order);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateStatusDto dto)
    {
        var order = await orderService.UpdateStatusAsync(id, dto.Status);
        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        await orderService.DeleteAsync(id);
        return NoContent();
    }
}