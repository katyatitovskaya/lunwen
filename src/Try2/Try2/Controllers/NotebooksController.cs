using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Extentions;
using Try2.Models;
using Try2.Models.DTOs;
using Try2.Models.Enums;

namespace Try2.Controllers
{
    [Authorize]
    public class NotebooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotebooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📒 список тетрадей
        public async Task<IActionResult> Index()
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

            return View(notebooks);
        }

        // ➕ создать тетрадь
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            string description,
            string text)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Название обязательно");
                return View();
            }

            var notebook = new Notebook
            {
                Name = name,
                Description = description,
                Text = text,
                CreationDate = DateTime.UtcNow
            };

            _context.Notebooks.Add(notebook);
            await _context.SaveChangesAsync();

            _context.StudyGroups.Add(new StudyGroup
            {
                NotebookId = notebook.Id,
                UserId = this.GetUserId(),
                UserRole = StudyGroupRole.Creator
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 📖 просмотр тетради
        public async Task<IActionResult> Details(int id)
        {
            var userId = this.GetUserId();

            var notebook = await _context.Notebooks
                .Include(n => n.StudyGroups)
                    .ThenInclude(sg => sg.User)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notebook == null)
                return NotFound();

            var myRole = notebook.StudyGroups
                .FirstOrDefault(sg => sg.UserId == userId);

            if (myRole == null)
                return Forbid();

            var model = new NotebookDetailsDto
            {
                Id = notebook.Id,
                Name = notebook.Name,
                Text = notebook.Text,
                Description = notebook.Description,
                CreationDate = notebook.CreationDate,
                CanEdit = myRole.UserRole == StudyGroupRole.Creator,
                Users = notebook.StudyGroups.Select(sg => new NotebookUserDto
                {
                    UserId = sg.UserId,
                    Username = sg.User.Username,
                    Nickname = sg.User.Nickname,
                    Role = sg.UserRole
                }).ToList()
            };

            return View(model);
        }

        // ✏️ редактирование
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = this.GetUserId();

            var notebook = await _context.Notebooks
                .Include(n => n.StudyGroups)
                    .ThenInclude(sg => sg.User)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notebook == null)
                return NotFound();

            var myRole = notebook.StudyGroups
                .FirstOrDefault(sg => sg.UserId == userId);

            if (myRole == null)
                return Forbid();

            var model = new NotebookDetailsDto
            {
                Id = notebook.Id,
                Name = notebook.Name,
                Description = notebook.Description,
                Text = notebook.Text,
                CreationDate = notebook.CreationDate,
                CanEdit = true,
                Users = notebook.StudyGroups.Select(sg => new NotebookUserDto
                {
                    UserId = sg.UserId,
                    Username = sg.User.Username,
                    Nickname = sg.User.Nickname,
                    Role = sg.UserRole
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NotebookDetailsDto model)
        {
            var userId = this.GetUserId();

            var notebook = await _context.Notebooks.FindAsync(model.Id);
            if (notebook == null)
                return NotFound();

            notebook.Name = model.Name;
            notebook.Description = model.Description;
            notebook.Text = model.Text;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }


        // 🗑 удалить тетрадь
        [HttpPost]
        public async Task<IActionResult> DeleteNotebook(int notebookId)
        {
            var userId = this.GetUserId();

            var isOwner = await _context.StudyGroups.AnyAsync(sg =>
                sg.NotebookId == notebookId &&
                sg.UserId == userId &&
                sg.UserRole == StudyGroupRole.Creator);

            if (!isOwner)
                return BadRequest("Только владелец может удалять тетради!");

            var notebook = await _context.Notebooks.FindAsync(notebookId);
            if (notebook == null)
                return NotFound();

            _context.Notebooks.Remove(notebook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(int notebookId, string username, StudyGroupRole role = StudyGroupRole.Participant)
        {
            var currentUserId = this.GetUserId();

            

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("Пользователь не найден");

            _context.StudyGroups.Add(new StudyGroup
            {
                NotebookId = notebookId,
                UserId = user.Id,
                UserRole = role
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = notebookId });
        }

        // 👥 удалить участника
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int notebookId, int userId)
        {
            var currentUserId = this.GetUserId();

            // ❌ запрет удалить самого себя
            if (userId == currentUserId)
                return BadRequest("Нельзя удалить самого себя");

            var sg = await _context.StudyGroups
                .FirstOrDefaultAsync(x =>
                    x.NotebookId == notebookId &&
                    x.UserId == userId);

            if (sg == null)
                return NotFound();

            // ❌ дополнительная защита: нельзя удалить создателя
            if (sg.UserRole == StudyGroupRole.Creator)
                return BadRequest("Нельзя удалить создателя тетради");

            _context.StudyGroups.Remove(sg);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = notebookId });
        }
    }
}