using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;
using Try2.Models.DTOs;

namespace Try2.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /Posts/Create
        public IActionResult Create()
        {
            return View(new PostDto());
        }

        // POST /Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("", "Текст поста не может быть пустым");
                return View(dto);
            }

            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var post = new Post
            {
                AuthorId = userId,
                Text = dto.Text,
                PublicationDate = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "User", new { id = userId });
        }
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
              .Include(p => p.Author)
              .Include(p => p.Likes)
              .Include(p => p.Comments)
                  .ThenInclude(c => c.User)
              .Include(p => p.Comments)
                  .ThenInclude(c => c.Likes)
              .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            int? currentUserId = null;
            if (User.Identity!.IsAuthenticated)
            {
                currentUserId = int.Parse(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
                );
            }
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.PostId = post.Id;

            var commentsTree = BuildCommentTree(post.Comments.ToList());

            var dto = new PostDto
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                AuthorUsername = post.Author.Username,
                AuthorNickname = post.Author.Nickname,
                Text = post.Text,
                PublicationDate = post.PublicationDate,
                LikesCount = post.Likes.Count,
                IsLikedByCurrentUser = currentUserId != null &&
                    post.Likes.Any(l => l.UserId == currentUserId),
                Comments = commentsTree
            };
            return View(dto);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Details", new { id = postId });

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );
            var comment = new Comment
            {
                PostId = postId,
                AuthorId = userId,
                Text = text,
                PublicationTime = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }

        private List<CommentDto> BuildCommentTree(List<Comment> comments)
        {
            var dict = comments.ToDictionary(c => c.Id);

            var result = new List<CommentDto>();

            var dtoMap = comments.ToDictionary(
                c => c.Id,
                c => new CommentDto
                {
                    Id = c.Id,
                    AuthorId = c.AuthorId,
                    Text = c.Text,
                    AuthorUsername = c.User.Username,
                    AuthorNickname = c.User.Nickname,
                    PublicationTime = c.PublicationTime,
                    Likes = c.Likes,
                    Replies = new List<CommentDto>()
                });

            foreach (var c in comments)
            {
                if (c.ParentCommentId == null)
                {
                    result.Add(dtoMap[c.Id]);
                }
                else
                {
                    dtoMap[c.ParentCommentId.Value]
                        .Replies
                        .Add(dtoMap[c.Id]);
                }
            }
            return result;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (post.AuthorId != userId) return Forbid();

            return View(new PostDto
            {
                Id = post.Id,
                Text = post.Text
            });
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostDto dto)
        {
            var post = await _context.Posts.FindAsync(dto.Id);
            if (post == null) return NotFound();

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (post.AuthorId != userId) return Forbid();

            post.Text = dto.Text;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = post.Id });
        }

        // 👍 лайк поста
        [HttpPost]
        public async Task<IActionResult> Like(int postId)
        {
            var userId = this.GetUserId();

            if (await _context.PostLikes.AnyAsync(l =>
                l.PostId == postId && l.UserId == userId))
                return BadRequest("Already liked");

            _context.PostLikes.Add(new PostLike
            {
                PostId = postId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = postId });
        }

        // 👎 убрать лайк
        [HttpPost]
        public async Task<IActionResult> Unlike(int postId)
        {
            var userId = this.GetUserId();

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(l =>
                    l.PostId == postId && l.UserId == userId);

            if (like == null) return NotFound();

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = this.GetUserId();

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            if (post.AuthorId != userId) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
