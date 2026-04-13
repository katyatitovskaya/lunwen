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
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(PostDto dto, string PostTagIds)
        //{
        //    // Отладка
        //    System.Diagnostics.Debug.WriteLine($"PostTagIds: {PostTagIds}");

        //    if (string.IsNullOrWhiteSpace(dto.Text))
        //    {
        //        ModelState.AddModelError("", "Текст поста не может быть пустым");
        //        return View(dto);
        //    }

        //    var userId = int.Parse(
        //        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
        //    );

        //    var post = new Post
        //    {
        //        AuthorId = userId,
        //        Text = dto.Text,
        //        PublicationDate = DateTime.UtcNow
        //    };

        //    _context.Posts.Add(post);
        //    await _context.SaveChangesAsync();

        //    // Добавляем теги из строки PostTagIds
        //    if (!string.IsNullOrEmpty(PostTagIds))
        //    {
        //        var tagIdArray = PostTagIds.Split(',').Select(int.Parse).ToArray();
        //        int priority = 1;
        //        foreach (var tagId in tagIdArray)
        //        {
        //            _context.PostTags.Add(new PostTag
        //            {
        //                PostId = post.Id,
        //                MainTagId = tagId,
        //                Priority = priority++
        //            });
        //        }
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Profile", "User", new { id = userId });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostDto dto)
        {
            // Полная отладка
            System.Diagnostics.Debug.WriteLine("========== ВСЕ ДАННЫЕ ФОРМЫ ==========");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"{key} = '{Request.Form[key]}'");
            }
            System.Diagnostics.Debug.WriteLine("======================================");

            string PostTagIds = Request.Form["PostTagIds"].ToString();
            System.Diagnostics.Debug.WriteLine($"PostTagIds из Request.Form: '{PostTagIds}'");
            System.Diagnostics.Debug.WriteLine($"PostTagIds пустой: {string.IsNullOrEmpty(PostTagIds)}");
            System.Diagnostics.Debug.WriteLine($"PostTagIds длина: {PostTagIds.Length}");

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

            System.Diagnostics.Debug.WriteLine($"Пост создан с ID: {post.Id}");

            if (!string.IsNullOrEmpty(PostTagIds))
            {
                System.Diagnostics.Debug.WriteLine($"Начинаем добавление тегов: '{PostTagIds}'");

                try
                {
                    var tagIdArray = PostTagIds.Split(',').Select(int.Parse).ToArray();
                    System.Diagnostics.Debug.WriteLine($"Распарсено тегов: {tagIdArray.Length}");

                    int priority = 1;
                    foreach (var tagId in tagIdArray)
                    {
                        System.Diagnostics.Debug.WriteLine($"Добавление PostTag: PostId={post.Id}, MainTagId={tagId}, Priority={priority}");

                        var postTag = new PostTag
                        {
                            PostId = post.Id,
                            MainTagId = tagId,
                            Priority = priority++
                        };
                        _context.PostTags.Add(postTag);
                    }

                    var savedCount = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Сохранено в базу: {savedCount} записей");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА при добавлении тегов: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("PostTagIds пустой - теги не добавляются");
            }

            return RedirectToAction("Profile", "User", new { id = userId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
              .Include(p => p.Author)
              .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
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

            var postTags = post.Tags
            .OrderBy(pt => pt.Priority)
            .Select(pt => new PostTagDto
            {
                Id = pt.Id,
                MainTagId = pt.MainTagId,
                TagName = pt.Tag.Name,
                PostId = pt.PostId,
                Priority = pt.Priority
            })
            .ToList();


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
                Comments = commentsTree,
                PostTags = postTags
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

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = this.GetUserId();
            var currentPost = 0;
            foreach(var comm in _context.Comments)
            {
                if(comm.Id == id)
                {
                    currentPost = comm.PostId;
                }
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            if (comment.AuthorId != userId) return Forbid();

            if (comment.Replies != null)
            {
                foreach (var reply in comment.Replies)
                {
                    _context.Comments.Remove(reply);
                }
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new {Id = currentPost});
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
                    AuthorProfilePhoto = c.User.ProfilePhoto,
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
            var post = await _context.Posts
            .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (post.AuthorId != userId) return Forbid();

            var postTags = post.Tags
                .OrderBy(pt => pt.Priority)
                .Select(pt => new PostTagDto
                {
                    Id = pt.Id,
                    MainTagId = pt.MainTagId,
                    TagName = pt.Tag.Name,
                    PostId = pt.PostId,
                    Priority = pt.Priority
                })
                .ToList();


            return View(new PostDto
            {
                Id = post.Id,
                Text = post.Text,
                PostTags = postTags
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

        // лайк поста
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

        // убрать лайк
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
            var currentUser = await _context.Users.FindAsync(userId);

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            if (post.AuthorId != userId && currentUser.IsAdmin == false) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Favourites()
        {
            List<PostDto>? likedPosts = null;

            if (User.Identity.IsAuthenticated)
            {
                int currentUserId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                );
                likedPosts = await _context.Posts
                .Include(p => p.Author)
                .Where(p =>
                    _context.PostLikes.Any(u =>
                        u.UserId == currentUserId
                        && u.PostId == p.Id))
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
                    IsLikedByCurrentUser = true
                })
                .ToListAsync();
            }
            return View(new FeedDto
            {
                LikedPosts = likedPosts
            });
        }
    }
}
