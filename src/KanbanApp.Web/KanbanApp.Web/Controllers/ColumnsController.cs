using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.Web.Controllers
{
    [Authorize]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class ColumnsController : ControllerBase
    {
        private readonly IColumnService _columnService;

        public ColumnsController(IColumnService columnService)
        {
            _columnService = columnService;
        }

        // GET: api/columns/board/{boardId}
        [HttpGet("board/{boardId:guid}")]
        public async Task<ActionResult<IEnumerable<KanbanColumnDto>>> GetColumnsByBoard(Guid boardId)
        {
            var columns = await _columnService.GetColumnsByBoardIdAsync(boardId);
            return Ok(columns);
        }

        // POST: api/columns
        [HttpPost]
        public async Task<ActionResult<KanbanColumnDto>> CreateColumn([FromBody] CreateColumnDto createDto)
        {
            var createdColumn = await _columnService.CreateColumnAsync(createDto);
            
            // Note: Returning Ok here because we don't have a single GetColumn endpoint yet,
            // otherwise we would use CreatedAtAction
            return Ok(createdColumn);
        }

        // PUT: api/columns/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnDto updateDto)
        {
            await _columnService.UpdateColumnAsync(id, updateDto);
            return NoContent();
        }

        // DELETE: api/columns/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteColumn(Guid id)
        {
            await _columnService.DeleteColumnAsync(id);
            return NoContent();
        }
    }
}