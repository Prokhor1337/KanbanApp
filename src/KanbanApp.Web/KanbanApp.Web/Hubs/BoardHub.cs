using KanbanApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KanbanApp.Web.Hubs
{
    [Authorize]
    public class BoardHub : Hub
    {
        private readonly IBoardRepository _boardRepo;

        public BoardHub(IBoardRepository boardRepo)
        {
            _boardRepo = boardRepo;
        }

        public async Task JoinBoardGroup(Guid boardId)
        {
            var userId = Context.UserIdentifier;
            var board = await _boardRepo.GetByIdAsync(boardId);

            // Allow access only to the board owner
            if (board == null || board.OwnerId != userId)
            {
                await Clients.Caller.SendAsync("AccessDenied", boardId);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());
        }

        public async Task LeaveBoardGroup(Guid boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId.ToString());
        }
    }
}