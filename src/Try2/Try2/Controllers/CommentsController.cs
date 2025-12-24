using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;

namespace Try2.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ добавить комментарий
        [HttpPost]
        public async Task<IActionResult> Create(
            int postId,
            string text,
            int? parentCommentId = null)
        {
            var comment = new Comment
            {
                PostId = postId,
                Text = text,
                AuthorId = this.GetUserId(),
                ParentCommentId = parentCommentId,
                PublicationTime = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment.Id);
        }

        // 📥 получить комментарии поста
        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByPost(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.PublicationTime)
                .Select(c => new
                {
                    c.Id,
                    c.Text,
                    c.AuthorId,
                    c.ParentCommentId,
                    c.PublicationTime
                })
                .ToListAsync();

            return Ok(comments);
        }

        // ❌ удалить комментарий
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = this.GetUserId();

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            if (comment.AuthorId != userId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

}
