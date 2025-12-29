using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Models.DTOs;
using System.Security.Claims;

namespace Try2.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        // Профиль пользователя
        public async Task<IActionResult> Profile(int id)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            int? currentUserId = GetCurrentUserId();

            bool isCurrentUser = currentUserId == user.Id;

            bool isFollowed = false;
            if (currentUserId != null && !isCurrentUser)
            {
                isFollowed = await _context.Subscriptions.AnyAsync(s =>
                    s.FollowerId == currentUserId &&
                    s.TargetUserId == user.Id);
            }

            var vm = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Role = user.Role,
                Bio = user.Bio,
                ProfilePhotoPath = user.ProfilePhoto,

                FollowersCount = await _context.Subscriptions
                    .CountAsync(s => s.TargetUserId == user.Id),

                FollowingCount = await _context.Subscriptions
                    .CountAsync(s => s.FollowerId == user.Id),

                IsCurrentUser = isCurrentUser,
                IsFollowedByCurrentUser = isFollowed,

                Posts = user.Posts
                    .OrderByDescending(p => p.PublicationDate)
                    .Select(p => new PostDto
                    {
                        Id = p.Id,
                        Text = p.Text,
                        PublicationDate = p.PublicationDate,
                        LikesCount = p.Likes.Count
                    })
                    .ToList()
            };

            return View(vm);
        }

        // Подписаться
        [HttpPost]
        public async Task<IActionResult> Follow(int userId)
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Login", "Account");

            bool alreadyFollow = await _context.Subscriptions.AnyAsync(s =>
                s.FollowerId == currentUserId &&
                s.TargetUserId == userId);

            if (!alreadyFollow)
            {
                _context.Subscriptions.Add(new Models.Subscription
                {
                    FollowerId = currentUserId.Value,
                    TargetUserId = userId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile", new { id = userId });
        }
        // Отписаться
        [HttpPost]
        public async Task<IActionResult> Unfollow(int userId)
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Login", "Account");

            var sub = await _context.Subscriptions.FirstOrDefaultAsync(s =>
                s.FollowerId == currentUserId &&
                s.TargetUserId == userId);

            if (sub != null)
            {
                _context.Subscriptions.Remove(sub);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile", new { id = userId });
        }

        private int? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(currentUserId.Value);
            if (user == null)
                return NotFound();

            return View(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Bio = user.Bio,
                ProfilePhotoPath = user.ProfilePhoto
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileDto dto)
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId != dto.Id)
                return Forbid();

            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Users.FindAsync(dto.Id);
            if (user == null)
                return NotFound();

            // проверка username
            bool usernameTaken = await _context.Users.AnyAsync(u =>
                u.Username == dto.Username && u.Id != dto.Id);

            if (usernameTaken)
            {
                ModelState.AddModelError("Username", "Этот username уже занят");
                return View(dto);
            }

            user.Username = dto.Username;
            user.Nickname = dto.Nickname;
            user.Bio = dto.Bio;

            // 📸 ЗАГРУЗКА ФОТО
            if (dto.ProfilePhoto != null && dto.ProfilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/avatars");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileExtension = Path.GetExtension(dto.ProfilePhoto.FileName);
                var fileName = $"user_{user.Id}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePhoto.CopyToAsync(stream);
                }

                user.ProfilePhoto = $"/uploads/avatars/{fileName}";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = user.Id });
        }


    }
}