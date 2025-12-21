using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyLunWen.Models;
using MyLunWen.Models.ViewModels;

namespace MyLunWen.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, не занят ли юзернейм
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Этот юзернейм уже занят");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Username,
                    Nickname = model.Nickname,
                    Role = model.Role,
                    About = model.About
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Пользователь создал новую учетную запись.");

                    // Добавляем пользователя в роль
                    await _userManager.AddToRoleAsync(user, model.Role.ToString());

                    // Автоматический вход после регистрации
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation("Пользователь вошел в систему после регистрации.");

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Пытаемся найти пользователя по username
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    // Если не нашли по username, пробуем по email
                    user = await _userManager.FindByEmailAsync(model.Username);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Пользователь вошел в систему.");

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Пользователь вышел из системы.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                Username = user.UserName,
                Nickname = user.Nickname,
                Role = user.Role.ToString(),
                About = user.About,
                ProfilePhoto = user.ProfilePhoto
            };

            return View(model);
        }

        // GET: /Account/EditProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EditProfileViewModel
            {
                Nickname = user.Nickname,
                About = user.About,
                ProfilePhoto = user.ProfilePhoto
            };

            return View(model);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                user.Nickname = model.Nickname;
                user.About = model.About;
                user.ProfilePhoto = model.ProfilePhoto;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Пользователь обновил свой профиль.");
                    TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    model.OldPassword,
                    model.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    _logger.LogInformation("Пользователь изменил свой пароль.");
                    TempData["SuccessMessage"] = "Пароль успешно изменен!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // AJAX проверка уникальности юзернейма
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                return Json($"Юзернейм '{username}' уже занят");
            }

            // Проверка формата
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 30)
            {
                return Json("Юзернейм должен быть от 3 до 30 символов");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
            {
                return Json("Только буквы, цифры, точки и нижние подчеркивания");
            }

            return Json(true);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
