using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Try2.Data;
using Try2.Models;
using Try2.Models.DTOs;

namespace Try2.Controllers
{
    [Authorize]
    public class NotebookPagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotebookPagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int notebookId)
        {
            var notebook = await _context.Notebooks
                .Include(n => n.Pages)
                .FirstOrDefaultAsync(n => n.Id == notebookId);

            if (notebook == null)
                return NotFound();

            var page = new NotebookPage
            {
                NotebookId = notebookId,
                Theme = "Без темы",
                CreationDate = DateTime.UtcNow,
                JsonContents = "" // Пустое содержимое
            };

            _context.NotebookPages.Add(page);
            await _context.SaveChangesAsync();

            // После создания сразу открываем страницу для редактирования
            return RedirectToAction(nameof(Details), new { id = page.Id });
        }

        // Просмотр и редактирование страницы
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var page = await _context.NotebookPages
                .Include(p => p.Notebook)
                .ThenInclude(n => n.Pages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
                return NotFound();

            // Получаем все страницы тетради, отсортированные по дате создания
            var pages = page.Notebook.Pages
                .OrderBy(p => p.CreationDate)
                .ToList();

            var currentIndex = pages.FindIndex(p => p.Id == id);

            var model = new NotebookPageDto
            {
                Id = page.Id,
                Theme = page.Theme,
                CreationDate = page.CreationDate,
                JsonContents = page.JsonContents,
                NotebookId = page.NotebookId,
                CurrentPageIndex = currentIndex + 1,
                TotalPages = pages.Count,
                HasPreviousPage = currentIndex > 0,
                HasNextPage = currentIndex < pages.Count - 1,
                PreviousPageId = currentIndex > 0 ? pages[currentIndex - 1].Id : null,
                NextPageId = currentIndex < pages.Count - 1 ? pages[currentIndex + 1].Id : null
            };

            return View(model);
        }

        // Сохранение изменений страницы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, string theme, string jsonContents)
        {
            var page = await _context.NotebookPages.FindAsync(id);

            if (page == null)
                return NotFound();

            page.Theme = string.IsNullOrWhiteSpace(theme) ? "Без темы" : theme;
            page.JsonContents = jsonContents ?? "";

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Переход к следующей странице
        [HttpGet]
        public async Task<IActionResult> Next(int currentPageId)
        {
            var currentPage = await _context.NotebookPages
                .Include(p => p.Notebook)
                .ThenInclude(n => n.Pages)
                .FirstOrDefaultAsync(p => p.Id == currentPageId);

            if (currentPage == null)
                return NotFound();

            var pages = currentPage.Notebook.Pages
                .OrderBy(p => p.CreationDate)
                .ToList();

            var currentIndex = pages.FindIndex(p => p.Id == currentPageId);

            if (currentIndex < pages.Count - 1)
            {
                return RedirectToAction(nameof(Details), new { id = pages[currentIndex + 1].Id });
            }

            return RedirectToAction(nameof(Details), new { id = currentPageId });
        }

        // Переход к предыдущей странице
        [HttpGet]
        public async Task<IActionResult> Previous(int currentPageId)
        {
            var currentPage = await _context.NotebookPages
                .Include(p => p.Notebook)
                .ThenInclude(n => n.Pages)
                .FirstOrDefaultAsync(p => p.Id == currentPageId);

            if (currentPage == null)
                return NotFound();

            var pages = currentPage.Notebook.Pages
                .OrderBy(p => p.CreationDate)
                .ToList();

            var currentIndex = pages.FindIndex(p => p.Id == currentPageId);

            if (currentIndex > 0)
            {
                return RedirectToAction(nameof(Details), new { id = pages[currentIndex - 1].Id });
            }

            return RedirectToAction(nameof(Details), new { id = currentPageId });
        }

    }
}
