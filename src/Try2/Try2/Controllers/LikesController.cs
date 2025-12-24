using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;

namespace Try2.Controllers
{
    [ApiController]
    [Route("api/likes")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LikesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 👍 лайк поста
        [HttpPost("post/{postId}")]
        public async Task<IActionResult> LikePost(int postId)
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
            return Ok();
        }

        // 👍 лайк комментария
        [HttpPost("comment/{commentId}")]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var userId = this.GetUserId();

            if (await _context.CommentLikes.AnyAsync(l =>
                l.CommentId == commentId && l.UserId == userId))
                return BadRequest("Already liked");

            _context.CommentLikes.Add(new CommentLike
            {
                CommentId = commentId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
            return Ok();
        }
        // 👎 убрать лайк
        [HttpDelete("post/{postId}")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = this.GetUserId();

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(l =>
                    l.PostId == postId && l.UserId == userId);

            if (like == null) return NotFound();

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> UnlikeComment(int commentId)
        {
            var userId = this.GetUserId();

            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(l =>
                    l.CommentId == commentId && l.UserId == userId);

            if (like == null) return NotFound();

            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}

