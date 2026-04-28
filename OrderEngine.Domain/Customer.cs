namespace OrderEngine.Domain.Entities;
public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public List<Order> Orders { get; set; } = new();
}