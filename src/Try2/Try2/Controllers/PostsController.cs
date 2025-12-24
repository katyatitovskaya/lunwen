using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;

namespace Try2.Controllers
{
    [ApiController]
    [Route("api/posts")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string text)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var post = new Post
            {
                AuthorId = userId,
                Text = text,
                PublicationDate = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post.Id);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.PublicationDate)
                .Select(p => new
                {
                    p.Id,
                    p.Text,
                    p.PublicationDate,
                    Author = p.Author.Username
                })
                .ToListAsync());
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts(int userId)
        {
            var posts = await _context.Posts
                .Where(p => p.AuthorId == userId)
                .OrderByDescending(p => p.PublicationDate)
                .Select(p => new
                {
                    p.Id,
                    p.Text,
                    p.PublicationDate
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("liked/{userId}")]
        public async Task<IActionResult> GetAllLikedPosts(int userId)
        {
            var posts = await _context.PostLikes
                .Where(l => l.UserId == userId)
                .Select(l => new
                {
                    l.Post.Id,
                    l.Post.Text,
                    l.Post.PublicationDate
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> EditPost(int postId, string text)
        {
            var userId = this.GetUserId();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            if (post.AuthorId != userId) return Forbid();

            post.Text = text;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = this.GetUserId();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            if (post.AuthorId != userId) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }

}
