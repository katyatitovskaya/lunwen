using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Models;
using Try2.Models.DTOs;
using Try2.Models.Enums;
using Try2.Models.Services;

namespace Try2.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(ApplicationDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                Nickname = dto.Nickname,
                Password = PasswordHasher.Hash(dto.Password),
                Role = dto.Role,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !PasswordHasher.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                user.Id,
                user.Username,
                user.Nickname
            });
        }
    }
        
}
