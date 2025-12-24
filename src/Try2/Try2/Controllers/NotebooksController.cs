using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;
using Try2.Models.Enums;

namespace Try2.Controllers
{
    [ApiController]
    [Route("api/notebooks")]
    [Authorize]
    public class NotebooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotebooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ создать тетрадь
        [HttpPost]
        public async Task<IActionResult> Create(string name, string text)
        {
            var notebook = new Notebook
            {
                Name = name,
                Text = text,
                CreationDate = DateTime.UtcNow
            };

            _context.Notebooks.Add(notebook);
            await _context.SaveChangesAsync();

            // создатель — владелец
            _context.StudyGroups.Add(new StudyGroup
            {
                NotebookId = notebook.Id,
                UserId = this.GetUserId(),
                UserRole = StudyGroupRole.Creator,
            });

            await _context.SaveChangesAsync();

            return Ok(notebook.Id);
        }

        // 📥 мои тетради
        [HttpGet("my")]
        public async Task<IActionResult> MyNotebooks()
        {
            var userId = this.GetUserId();

            var notebooks = await _context.StudyGroups
                .Where(sg => sg.UserId == userId)
                .Select(sg => new
                {
                    sg.Notebook.Id,
                    sg.Notebook.Name,
                    sg.UserRole
                })
                .ToListAsync();

            return Ok(notebooks);
        }

        // 👥 добавить участника
        [HttpPost("{notebookId}/users/{userId}")]
        public async Task<IActionResult> AddUser(
            int notebookId,
            int userId,
            StudyGroupRole role = StudyGroupRole.Participant)
        {
            var currentUserId = this.GetUserId();

            var owner = await _context.StudyGroups.AnyAsync(sg =>
                sg.NotebookId == notebookId &&
                sg.UserId == currentUserId &&
                sg.UserRole == StudyGroupRole.Creator);

            if (!owner)
                return Forbid();

            _context.StudyGroups.Add(new StudyGroup
            {
                NotebookId = notebookId,
                UserId = userId,
                UserRole = role
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{notebookId}")]
        public async Task<IActionResult> EditNotebook(int notebookId, string name, string text)
        {
            var userId = this.GetUserId();

            var isOwner = await _context.StudyGroups.AnyAsync(sg =>
                sg.NotebookId == notebookId &&
                sg.UserId == userId &&
                sg.UserRole == StudyGroupRole.Creator);

            if (!isOwner) return Forbid();

            var notebook = await _context.Notebooks.FindAsync(notebookId);
            if (notebook == null) return NotFound();

            notebook.Name = name;
            notebook.Text = text;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{notebookId}")]
        public async Task<IActionResult> DeleteNotebook(int notebookId)
        {
            var userId = this.GetUserId();

            var isOwner = await _context.StudyGroups.AnyAsync(sg =>
                sg.NotebookId == notebookId &&
                sg.UserId == userId &&
                sg.UserRole == StudyGroupRole.Creator);

            if (!isOwner) return Forbid();

            var notebook = await _context.Notebooks.FindAsync(notebookId);
            if (notebook == null) return NotFound();

            _context.Notebooks.Remove(notebook);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{notebookId}/users/{userId}")]
        public async Task<IActionResult> RemoveUser(int notebookId, int userId)
        {
            var currentUserId = this.GetUserId();

            var owner = await _context.StudyGroups.AnyAsync(sg =>
                sg.NotebookId == notebookId &&
                sg.UserId == currentUserId &&
                sg.UserRole == StudyGroupRole.Creator);

            if (!owner) return Forbid();

            var sg = await _context.StudyGroups
                .FirstOrDefaultAsync(x =>
                    x.NotebookId == notebookId && x.UserId == userId);

            if (sg == null) return NotFound();

            _context.StudyGroups.Remove(sg);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
