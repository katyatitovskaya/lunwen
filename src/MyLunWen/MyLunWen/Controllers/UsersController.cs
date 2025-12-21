using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLunWen.Data;
using MyLunWen.Models;
using System.Threading.Tasks;
using MyLunWen.Models.ViewModels;

namespace MyLunWen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchString, string roleFilter, int page = 1, int pageSize = 10)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentRoleFilter = roleFilter;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;

            var usersQuery = _userManager.Users.AsQueryable();

            // Фильтр по поиску
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.Nickname.Contains(searchString));
            }

            // Фильтр по роли
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "Все")
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var userIds = usersInRole.Select(u => u.Id);
                usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
            }

            // Пагинация
            var totalUsers = await usersQuery.CountAsync();
            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Nickname = u.Nickname,
                    Role = u.Role,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now
                })
                .ToListAsync();

            // Получаем роли для каждого пользователя
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(appUser);
                user.Roles = roles.ToList();
            }

            var model = new UserListViewModel
            {
                Users = users,
                TotalCount = totalUsers,
                PageSize = pageSize,
                CurrentPage = page,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserDetailViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Nickname = user.Nickname,
                Role = user.Role,
                About = user.About,
                ProfilePhoto = user.ProfilePhoto,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles.ToList(),
                AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var model = new CreateUserViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };
            return View(model);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Проверка уникальности username
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
                    Role = model.Role
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Добавляем пользователя в выбранные роли
                    if (!string.IsNullOrEmpty(model.Role.ToString()))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role.ToString());
                    }

                    _logger.LogInformation("Администратор создал нового пользователя: {Username}", model.Username);
                    TempData["SuccessMessage"] = $"Пользователь {model.Username} успешно создан!";

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Nickname = user.Nickname,
                Role = user.Role,
                About = user.About,
                ProfilePhoto = user.ProfilePhoto,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                
                user.Nickname = model.Nickname;
                user.Role = model.Role;
                user.About = model.About;
                user.ProfilePhoto = model.ProfilePhoto;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Обновляем роли
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(model.Role.ToString()))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role.ToString());
                    }

                    _logger.LogInformation("Администратор обновил пользователя: {Username}", model.Username);
                    TempData["SuccessMessage"] = $"Пользователь {model.Username} успешно обновлен!";

                    return RedirectToAction("Details", new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Нельзя удалить самого себя
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Вы не можете удалить свой собственный аккаунт!";
                return RedirectToAction("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Администратор удалил пользователя: {Username}", user.UserName);
                TempData["SuccessMessage"] = $"Пользователь {user.UserName} успешно удален!";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении пользователя";
            }

            return RedirectToAction("Index");
        }

        // POST: Users/Lock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, int days = 7)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Нельзя заблокировать самого себя
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Вы не можете заблокировать свой собственный аккаунт!";
                return RedirectToAction("Details", new { id });
            }

            var lockoutEnd = DateTimeOffset.Now.AddDays(days);
            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

            _logger.LogInformation("Администратор заблокировал пользователя {Username} до {LockoutEnd}",
                user.UserName, lockoutEnd);
            TempData["SuccessMessage"] = $"Пользователь {user.UserName} заблокирован до {lockoutEnd:dd.MM.yyyy}";

            return RedirectToAction("Details", new { id });
        }

        // POST: Users/Unlock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Администратор разблокировал пользователя {Username}", user.UserName);
            TempData["SuccessMessage"] = $"Пользователь {user.UserName} разблокирован";

            return RedirectToAction("Details", new { id });
        }

        // POST: Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Генерируем токен сброса пароля
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Администратор сбросил пароль пользователя {Username}", user.UserName);
                TempData["SuccessMessage"] = $"Пароль пользователя {user.UserName} успешно сброшен";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при сбросе пароля";
            }

            return RedirectToAction("Details", new { id });
        }

        // POST: Users/AddToRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["ErrorMessage"] = $"Роль '{roleName}' не найдена";
                return RedirectToAction("Details", new { id });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Администратор добавил пользователя {Username} в роль {Role}",
                    user.UserName, roleName);
                TempData["SuccessMessage"] = $"Пользователь {user.UserName} добавлен в роль {roleName}";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при добавлении роли";
            }

            return RedirectToAction("Details", new { id });
        }

        // POST: Users/RemoveFromRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Нельзя удалить роль Admin у самого себя
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId && roleName == "Admin")
            {
                TempData["ErrorMessage"] = "Вы не можете удалить роль Admin у своего аккаунта!";
                return RedirectToAction("Details", new { id });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Администратор удалил пользователя {Username} из роли {Role}",
                    user.UserName, roleName);
                TempData["SuccessMessage"] = $"Пользователь {user.UserName} удален из роли {roleName}";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении роли";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
