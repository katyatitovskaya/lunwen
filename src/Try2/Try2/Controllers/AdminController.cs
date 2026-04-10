using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using Try2.Attributes;
using Try2.Data;
using Try2.Models.DTOs;
using Try2.Extentions;

namespace Try2.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> TagsIndex()
        {
            var allTags = await _context.Tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsConfirmed = t.IsConfirmed,
                    PostsCount = t.PostTags.Count(),
                    UsersCount = t.UserTags.Count()
                })
                .OrderBy(t => t.IsConfirmed)     
                .ThenByDescending(t => t.PostsCount + t.UsersCount)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(allTags);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            tag.IsConfirmed = true;
            tag.ConfirmedAt = DateTime.UtcNow;
            tag.ConfirmedByUserId = this.GetUserId();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
