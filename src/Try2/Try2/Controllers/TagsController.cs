using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Models;
using Try2.Models.DTOs;
using Try2.Models.Enums;

namespace Try2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Поиск тегов для автодополнения
        [HttpGet("search")]
        public async Task<IActionResult> SearchTags([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
                return Ok(new List<object>());

            var tags = await _context.Tags
                .Where(t => t.Name.Contains(term))
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.IsConfirmed,
                    UsageCount = t.UserTags.Count() + t.PostTags.Count()
                })
                .OrderByDescending(t => t.Name.StartsWith(term))
                .ThenByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name)
                .Take(10)
                .ToListAsync();

            return Ok(tags);
        }

        // Получить теги пользователя
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTags(int userId)
        {
            var userTags = await _context.UserTags
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Tag)
                .Select(ut => new UserTagDto
                {
                    Id = ut.Id,
                    TagId = ut.MainTagId,
                    TagName = ut.Tag.Name,
                    StudyStartYear = ut.StudyStartYear,
                    Phase = ut.Phase,
                    IsConfirmed = ut.Tag.IsConfirmed
                })
                .OrderBy(ut => ut.TagName)
                .ToListAsync();

            return Ok(userTags);
        }

        // Добавить тег пользователю
        [HttpPost("user/add")]
        public async Task<IActionResult> AddUserTag([FromBody] AddTagRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            int tagId;

            // Если передан существующий ID тега
            if (request.TagId.HasValue && request.TagId > 0)
            {
                tagId = request.TagId.Value;
            }
            // Если передано название нового тега
            else if (!string.IsNullOrWhiteSpace(request.TagName))
            {
                // Проверяем, существует ли уже такой тег
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == request.TagName.Trim().ToLower());

                if (existingTag != null)
                {
                    tagId = existingTag.Id;
                }
                else
                {
                    // Создаем новый тег с IsConfirmed = false
                    var newTag = new Tag
                    {
                        Name = request.TagName.Trim(),
                        IsConfirmed = false
                    };
                    _context.Tags.Add(newTag);
                    await _context.SaveChangesAsync();
                    tagId = newTag.Id;
                }
            }
            else
            {
                return BadRequest("Не указан тег");
            }

            // Проверяем, нет ли уже такого тега у пользователя
            var existingUserTag = await _context.UserTags
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.MainTagId == tagId);

            if (existingUserTag != null)
            {
                return BadRequest("Этот тег уже добавлен");
            }

            var userTag = new UserTag
            {
                UserId = userId.Value,
                MainTagId = tagId,
                StudyStartYear = request.StudyStartYear,
                Phase = request.Phase
            };

            _context.UserTags.Add(userTag);
            await _context.SaveChangesAsync();

            // Загружаем созданный тег с данными
            var result = await _context.UserTags
                .Where(ut => ut.Id == userTag.Id)
                .Include(ut => ut.Tag)
                .Select(ut => new UserTagDto
                {
                    Id = ut.Id,
                    TagId = ut.MainTagId,
                    TagName = ut.Tag.Name,
                    StudyStartYear = ut.StudyStartYear,
                    Phase = ut.Phase,
                    IsConfirmed = ut.Tag.IsConfirmed
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }

        // Удалить тег у пользователя
        [HttpDelete("user/remove/{userTagId}")]
        public async Task<IActionResult> RemoveUserTag(int userTagId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var userTag = await _context.UserTags
                .FirstOrDefaultAsync(ut => ut.Id == userTagId && ut.UserId == userId);

            if (userTag == null)
                return NotFound();

            _context.UserTags.Remove(userTag);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }

        // Добавьте в конец класса TagsController, перед закрывающей скобкой

        // Добавить тег к посту
        [HttpPost("post/add")]
        public async Task<IActionResult> AddPostTag([FromBody] AddPostTagRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // Проверяем, что пост принадлежит пользователю
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == request.PostId && p.AuthorId == userId);

            if (post == null)
                return Forbid();

            // Проверяем, не добавлен ли уже такой тег
            var existing = await _context.PostTags
                .AnyAsync(pt => pt.PostId == request.PostId && pt.MainTagId == request.TagId);

            if (existing)
                return BadRequest("Тег уже добавлен");

            // Определяем следующий приоритет
            var maxPriority = await _context.PostTags
                .Where(pt => pt.PostId == request.PostId)
                .MaxAsync(pt => (int?)pt.Priority) ?? 0;

            var postTag = new PostTag
            {
                PostId = request.PostId,
                MainTagId = request.TagId,
                Priority = maxPriority + 1
            };

            _context.PostTags.Add(postTag);
            await _context.SaveChangesAsync();

            // Загружаем данные для ответа
            var tag = await _context.Tags.FindAsync(request.TagId);

            return Ok(new PostTagDto
            {
                Id = postTag.Id,
                MainTagId = postTag.MainTagId,
                TagName = tag.Name,
                PostId = postTag.PostId,
                Priority = postTag.Priority
            });
        }

        // Удалить тег из поста
        [HttpDelete("post/remove/{postTagId}")]
        public async Task<IActionResult> RemovePostTag(int postTagId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var postTag = await _context.PostTags
                .Include(pt => pt.Post)
                .FirstOrDefaultAsync(pt => pt.Id == postTagId);

            if (postTag == null)
                return NotFound();

            if (postTag.Post.AuthorId != userId)
                return Forbid();

            _context.PostTags.Remove(postTag);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetPostTags(int postId)
        {
            var postTags = await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .OrderBy(pt => pt.Priority)
                .Select(pt => new PostTagDto
                {
                    Id = pt.Id,
                    MainTagId = pt.MainTagId,
                    TagName = pt.Tag.Name,
                    PostId = pt.PostId,
                    Priority = pt.Priority
                })
                .ToListAsync();

            return Ok(postTags);
        }

        // Класс запроса
        public class AddPostTagRequest
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
        }


        // Класс для запроса добавления тега
        public class AddTagRequest
        {
            public int? TagId { get; set; }
            public string? TagName { get; set; }
            public int StudyStartYear { get; set; }
            public TagStudyPhase Phase { get; set; }
        }
    }
}