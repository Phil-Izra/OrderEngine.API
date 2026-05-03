using Microsoft.AspNetCore.Mvc;
using OrderEngine.Application.DTOs;
using OrderEngine.Application.Interfaces;
using OrderEngine.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IOrderRepository _repo;

    public OrdersController(OrderService orderService, IOrderRepository repo)
    {
        _orderService = orderService;
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _orderService.ProcessOrderAsync(dto);
        return Ok(order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _repo.GetAllAsync();
        return Ok(orders);
    }
}