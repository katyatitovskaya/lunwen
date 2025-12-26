using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;

namespace Try2.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Like(int commentId, int postId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var exists = await _context.CommentLikes.AnyAsync(l =>
                l.UserId == userId && l.CommentId == commentId);

            if (!exists)
            {
                _context.CommentLikes.Add(new CommentLike
                {
                    UserId = userId,
                    CommentId = commentId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Posts", new { id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> Unlike(int commentId, int postId)
        {
            var userId = this.GetUserId();

            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(l =>
                    l.CommentId == commentId && l.UserId == userId);

            if (like == null) return NotFound();

            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> Reply(
                    int postId,
                    int parentCommentId,
                    string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Details", "Posts", new { id = postId });

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            _context.Comments.Add(new Comment
            {
                PostId = postId,
                ParentCommentId = parentCommentId,
                AuthorId = userId,
                Text = text,
                PublicationTime = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}
