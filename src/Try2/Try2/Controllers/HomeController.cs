using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Try2.Data;
using Try2.Models.DTOs;
using Try2.ViewModels;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // ---------- бяе онярш ----------
        var allPosts = await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.PublicationDate)
            .Select(p => new PostDto
            {
                Id = p.Id,
                AuthorId = p.AuthorId,
                AuthorUsername = p.Author.Username,
                AuthorNickname = p.Author.Nickname,
                AuthorProfilePhoto = p.Author.ProfilePhoto,
                Text = p.Text,
                PublicationDate = p.PublicationDate,
                LikesCount = _context.PostLikes.Count(l =>
                     l.PostId == p.Id),
                IsLikedByCurrentUser = false
            })
            .ToListAsync();

        List<PostDto>? subscriptionPosts = null;

        // ---------- онярш ондохянй ----------
        if (User.Identity.IsAuthenticated)
        {
            int currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            subscriptionPosts = await _context.Posts
                .Include(p => p.Author)
                .Where(p =>
                    _context.Subscriptions.Any(s =>
                        s.FollowerId == currentUserId &&
                        s.TargetUserId == p.AuthorId))
                .OrderByDescending(p => p.PublicationDate)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    AuthorId = p.AuthorId,
                    AuthorUsername = p.Author.Username,
                    AuthorNickname = p.Author.Nickname,
                    AuthorProfilePhoto = p.Author.ProfilePhoto,
                    Text = p.Text,
                    PublicationDate = p.PublicationDate,
                    LikesCount = _context.PostLikes.Count(l =>
                        l.PostId == p.Id),
                    IsLikedByCurrentUser = _context.PostLikes.Any(l =>
                        l.PostId == p.Id &&
                        l.UserId == currentUserId)
                })
                .ToListAsync();
        }

        return View(new FeedViewModel
        {
            AllPosts = allPosts,
            SubscriptionPosts = subscriptionPosts
        });
    }
}
