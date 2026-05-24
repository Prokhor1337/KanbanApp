using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using KanbanApp.Web.Hubs; // Подключаем пространство имен нашего Хаба

namespace KanbanApp.Web.Controllers
{
    [Authorize]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IHubContext<BoardHub> _hubContext; // Добавляем контекст хаба

        // Инжектим хаб через конструктор
        public TasksController(ITaskService taskService, IHubContext<BoardHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(Guid id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpGet("column/{columnId:guid}")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksByColumn(Guid columnId)
        {
            var tasks = await _taskService.GetTasksByColumnIdAsync(columnId);
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskDto createDto)
        {
            var createdTask = await _taskService.CreateTaskAsync(createDto);
            
            // Сообщаем всем, что доска обновилась
            await _hubContext.Clients.All.SendAsync("BoardUpdated");
            
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto updateDto)
        {
            await _taskService.UpdateTaskAsync(id, updateDto);
            await _hubContext.Clients.All.SendAsync("BoardUpdated");
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            await _taskService.DeleteTaskAsync(id);
            await _hubContext.Clients.All.SendAsync("BoardUpdated");
            return NoContent();
        }

        [HttpPatch("{id:guid}/move")]
        public async Task<IActionResult> MoveTask(Guid id, [FromQuery] Guid targetColumnId, [FromQuery] int newOrder)
        {
            await _taskService.MoveTaskAsync(id, targetColumnId, newOrder);
            
            // МАГИЯ ЗДЕСЬ: Сообщаем всем клиентам о перемещении карточки
            await _hubContext.Clients.All.SendAsync("BoardUpdated");
            
            return NoContent();
        }
    }
}