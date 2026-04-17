using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Models.DTOs;

namespace Try2.Controllers
{
    public class SearchController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class SearchApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchApiController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchRequestDto request)
        {
            var result = new SearchResultDto();
            int? currentUserId = GetCurrentUserId();

            var query = request.Query?.Trim().ToLower() ?? "";

            // Поиск пользователей (если выбран тип All или Users)
            if (request.SearchType == "All" || request.SearchType == "Users")
            {
                var usersQuery = _context.Users
                    .Include(u => u.Tags)
                        .ThenInclude(ut => ut.Tag)
                    .AsQueryable();

                // Фильтр по тексту
                if (!string.IsNullOrEmpty(query))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Username.ToLower().Contains(query) ||
                        u.Nickname.ToLower().Contains(query));
                }

                // Фильтр по тегу
                if (request.TagId.HasValue)
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Tags.Any(ut => ut.MainTagId == request.TagId.Value));
                }

                // Фильтр по фазе изучения
                if (request.Phase.HasValue)
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Tags.Any(ut => ut.Phase == request.Phase.Value));
                }

                var users = await usersQuery
                    .Take(20)
                    .Select(u => new UserSearchResultDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Nickname = u.Nickname,
                        ProfilePhotoPath = u.ProfilePhoto,
                        IsFollowedByCurrentUser = currentUserId != null &&
                            _context.Subscriptions.Any(s =>
                                s.FollowerId == currentUserId &&
                                s.TargetUserId == u.Id),
                        Tags = u.Tags.Select(ut => new UserTagDto
                        {
                            Id = ut.Id,
                            TagId = ut.MainTagId,
                            TagName = ut.Tag.Name,
                            StudyStartYear = ut.StudyStartYear,
                            Phase = ut.Phase,
                            IsConfirmed = ut.Tag.IsConfirmed
                        }).ToList()
                    })
                    .ToListAsync();

                result.Users = users;
            }

            // Поиск постов (если выбран тип All или Posts)
            if (request.SearchType == "All" || request.SearchType == "Posts")
            {
                var postsQuery = _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.Tags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Фильтр по тексту
                if (!string.IsNullOrEmpty(query))
                {
                    postsQuery = postsQuery.Where(p =>
                        p.Text.ToLower().Contains(query));
                }

                // Фильтр по тегу
                if (request.TagId.HasValue)
                {
                    postsQuery = postsQuery.Where(p =>
                        p.Tags.Any(pt => pt.MainTagId == request.TagId.Value));
                }

                var posts = await postsQuery
                    .OrderByDescending(p => p.Tags.Any(pt => pt.MainTagId == request.TagId) ?
                        p.Tags.Where(pt => pt.MainTagId == request.TagId)
                              .Min(pt => pt.Priority) : int.MaxValue)
                    .ThenByDescending(p => p.PublicationDate)
                    .Take(30)
                    .Select(p => new PostSearchResultDto
                    {
                        Id = p.Id,
                        AuthorId = p.AuthorId,
                        AuthorUsername = p.Author.Username,
                        AuthorNickname = p.Author.Nickname,
                        AuthorProfilePhoto = p.Author.ProfilePhoto,
                        Text = p.Text.Length > 200 ? p.Text.Substring(0, 200) + "..." : p.Text,
                        PublicationDate = p.PublicationDate,
                        LikesCount = _context.PostLikes.Count(l => l.PostId == p.Id),
                        IsLikedByCurrentUser = currentUserId != null &&
                            _context.PostLikes.Any(l =>
                                l.PostId == p.Id && l.UserId == currentUserId),
                        Tags = p.Tags.OrderBy(pt => pt.Priority).Select(pt => new PostTagDto
                        {
                            Id = pt.Id,
                            MainTagId = pt.MainTagId,
                            TagName = pt.Tag.Name,
                            PostId = pt.PostId,
                            Priority = pt.Priority
                        }).ToList()
                    })
                    .ToListAsync();

                result.Posts = posts;
            }

            return Ok(result);
        }

        [HttpGet("tags")]
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

        private int? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated) return null;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
