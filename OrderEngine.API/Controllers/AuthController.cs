using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderEngine.Application.DTOs;
using OrderEngine.Domain.Entities;
using OrderEngine.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists");
        var user = new AppUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.FullName,
        user.Email, user.PhotoUrl,
        DateTime.UtcNow.AddMinutes(60)));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(
        dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");
        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.FullName,
        user.Email, user.PhotoUrl,
        DateTime.UtcNow.AddMinutes(60)));
    }
    [HttpPost("upload-photo")]
    [Authorize]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        var userId = Guid.Parse(User.FindFirst(
        ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();
        var uploads = Path.Combine("wwwroot", "photos");
        Directory.CreateDirectory(uploads);
        var fileName = $"{userId}{Path.GetExtension(photo.FileName)}";
        var filePath = Path.Combine(uploads, fileName);
        using var stream = System.IO.File.Create(filePath);
        await photo.CopyToAsync(stream);
        user.PhotoUrl = $"/photos/{fileName}";
        await _db.SaveChangesAsync();
        return Ok(new { photoUrl = user.PhotoUrl });
    }
}