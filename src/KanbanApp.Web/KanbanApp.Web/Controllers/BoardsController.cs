using System.Security.Claims;
using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace KanbanApp.Web.Controllers
{
    [Authorize]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BoardsController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public BoardsController(IBoardService boardService, IMemoryCache cache,
            ILogger<BoardsController> logger, UserManager<AppUser> userManager)
        {
            _boardService = boardService;
            _cache = cache;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetMyBoards()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cacheKey = $"boards_{userId}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<BoardDto>? boards))
            {
                _logger.LogInformation("Cache miss for user {UserId}, loading from DB", userId);
                boards = await _boardService.GetUserBoardsAsync(userId);

                _cache.Set(cacheKey, boards, TimeSpan.FromSeconds(30));
            }
            else
            {
                _logger.LogInformation("Cache hit for user {UserId}", userId);
            }

            return Ok(boards);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BoardDto>> GetBoard(Guid id)
        {
            var board = await _boardService.GetBoardByIdAsync(id);
            if (board == null)
                return NotFound();
            return Ok(board);
        }

        [HttpPost]
        public async Task<ActionResult<BoardDto>> CreateBoard([FromBody] CreateBoardDto createDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var createdBoard = await _boardService.CreateBoardAsync(userId, createDto);

            // Инвалидируем кеш после создания новой доски
            _cache.Remove($"boards_{userId}");
            _logger.LogInformation("Cache invalidated for user {UserId} after board creation", userId);

            return CreatedAtAction(nameof(GetBoard), new { id = createdBoard.Id }, createdBoard);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _boardService.UpdateBoardAsync(id, updateDto);
            _cache.Remove($"boards_{userId}");
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _boardService.DeleteBoardAsync(id);
            _cache.Remove($"boards_{userId}");
            return NoContent();
        }
        
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetAllBoards()
        {
            // Direct DB role check — bypasses claim enrichment middleware issues
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin) return Forbid();

            _logger.LogInformation("Admin requested all boards");
            var boards = await _boardService.GetAllBoardsAsync();
            return Ok(boards);
        }
    }
}