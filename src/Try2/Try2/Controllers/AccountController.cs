// Try2.Controllers.AccountController (обновленная версия)
using Microsoft.AspNetCore.Mvc;
using Try2.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Models;
using Try2.Models.Services;

namespace Try2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // регистрация
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                ModelState.AddModelError("", "Пользователь уже существует");
                return View(dto);
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                ModelState.AddModelError("", "Email уже используется");
                return View(dto);
            }

            // Генерация 6-значного кода
            var confirmationCode = GenerateConfirmationCode();

            var user = new User
            {
                Username = dto.Username,
                Nickname = dto.Nickname,
                Email = dto.Email,
                Password = PasswordHasher.Hash(dto.Password),
                Role = dto.Role,
                IsEmailConfirmed = false,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15),
                IsAdmin = false // По умолчанию не админ
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Отправка кода на email
            try
            {
                await _emailService.SendConfirmationCodeAsync(user.Email, confirmationCode);
                TempData["SuccessMessage"] = "Код подтверждения отправлен на ваш email.";
                return RedirectToAction("ConfirmEmail", new { email = user.Email });
            }
            catch (Exception ex)
            {
                // В случае ошибки отправки email, удаляем пользователя
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Ошибка при отправке email. Пожалуйста, попробуйте позже.");
                return View(dto);
            }
        }

        // Подтверждение email
        [HttpGet]
        public IActionResult ConfirmEmail(string email)
        {
            var model = new ConfirmEmailDto { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsEmailConfirmed);

            if (user == null)
            {
                ModelState.AddModelError("", "Email не найден или уже подтвержден");
                return View(dto);
            }

            if (user.EmailConfirmationCode != dto.Code)
            {
                ModelState.AddModelError("", "Неверный код подтверждения");
                return View(dto);
            }

            if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Код подтверждения истек. Запросите новый код.");
                return View(dto);
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpiry = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Email успешно подтвержден! Теперь вы можете войти.";
            return RedirectToAction("Login");
        }

        // Повторная отправка кода
        [HttpPost]
        public async Task<IActionResult> ResendConfirmationCode(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsEmailConfirmed);

            if (user == null)
                return NotFound();

            // Генерация нового кода
            var confirmationCode = GenerateConfirmationCode();
            user.EmailConfirmationCode = confirmationCode;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await _emailService.SendConfirmationCodeAsync(user.Email, confirmationCode);

            TempData["SuccessMessage"] = "Новый код подтверждения отправлен на ваш email.";
            return RedirectToAction("ConfirmEmail", new { email = user.Email });
        }

        // вход в аккаунт
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !PasswordHasher.Verify(dto.Password, user.Password))
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(dto);
            }

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("IsAdmin", user.IsAdmin.ToString()) 
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        // выход из аккаунта
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private string GenerateConfirmationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}