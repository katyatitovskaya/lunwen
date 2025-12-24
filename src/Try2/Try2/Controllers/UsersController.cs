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
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Subscriptions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Nickname,
                Followers = user.Followers.Count,
                Following = user.Subscriptions.Count
            });
        }

        [HttpPost("{id}/follow")]
        public async Task<IActionResult> Follow(int id)
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            if (currentUserId == id)
                return BadRequest("Cannot follow yourself");

            if (await _context.Subscriptions.AnyAsync(s =>
                s.FollowerId == currentUserId && s.TargetUserId == id))
                return BadRequest("Already followed");

            _context.Subscriptions.Add(new Subscription
            {
                FollowerId = currentUserId,
                TargetUserId = id
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{userId}/unfollow")]
        [Authorize]
        public async Task<IActionResult> Unfollow(int userId)
        {
            var currentUserId = this.GetUserId();

            // нельзя отписаться от себя
            if (currentUserId == userId)
                return BadRequest("You cannot unfollow yourself");

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s =>
                    s.FollowerId == currentUserId &&
                    s.TargetUserId == userId);

            if (subscription == null)
                return NotFound("Subscription not found");

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{userId}/followers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFollowers(int userId)
        {
            var followers = await _context.Subscriptions
                .Where(s => s.TargetUserId == userId)
                .Select(s => new
                {
                    s.Follower.Id,
                    s.Follower.Username,
                    s.Follower.Nickname
                })
                .ToListAsync();

            return Ok(followers);
        }

        [HttpGet("{userId}/subscriptions")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSubscriptions(int userId)
        {
            var subs = await _context.Subscriptions
                .Where(s => s.FollowerId == userId)
                .Select(s => new
                {
                    s.TargetUser.Id,
                    s.TargetUser.Username,
                    s.TargetUser.Nickname
                })
                .ToListAsync();

            return Ok(subs);
        }

        [HttpGet("{userId}/friends")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersFriends(int userId)
        {
            var friends = await _context.Subscriptions
                .Where(s => s.FollowerId == userId)
                .Join(
                    _context.Subscriptions,
                    s => s.TargetUserId,
                    s2 => s2.FollowerId,
                    (s, s2) => new { s, s2 }
                )
                .Where(x => x.s2.TargetUserId == userId)
                .Select(x => new
                {
                    x.s.TargetUser.Id,
                    x.s.TargetUser.Username,
                    x.s.TargetUser.Nickname
                })
                .Distinct()
                .ToListAsync();

            return Ok(friends);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile(string nickname, string? bio, string? pfp)
        {
            var userId = this.GetUserId();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Nickname = nickname;
            user.Bio = bio;
            user.ProfilePhoto = pfp;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("profile")]
        [Authorize]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = this.GetUserId();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
