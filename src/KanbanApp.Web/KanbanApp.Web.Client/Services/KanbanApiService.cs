using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using KanbanApp.Application.DTOs;

namespace KanbanApp.Web.Client.Services
{
    public class KanbanApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        // Inject LocalStorage directly into the API service
        public KanbanApiService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        // Helper method to guarantee the token is attached before ANY request
        private async Task SetAuthorizationHeader()
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken"); // ← GetItemAsStringAsync!
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.Trim('"'); // strip JSON quotes if present
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// Returns the current user's ID by calling the server — reliable regardless of JWT claim mapping.
        public async Task<string?> GetCurrentUserIdAsync()
        {
            await SetAuthorizationHeader();
            try
            {
                var result = await _httpClient.GetFromJsonAsync<UserProfileDto>("api/account/me");
                return result?.Id;
            }
            catch { return null; }
        }

        /// Returns the full profile of the current user.
        public async Task<UserProfileDto?> GetProfileAsync()
        {
            await SetAuthorizationHeader();
            try { return await _httpClient.GetFromJsonAsync<UserProfileDto>("api/account/me"); }
            catch { return null; }
        }

        /// Updates the current user's display name.
        public async Task<(bool ok, string? error)> UpdateProfileAsync(string displayName)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync("api/account/me", new { displayName });
            if (response.IsSuccessStatusCode) return (true, null);
            try
            {
                var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, err?.Error ?? "Failed to update profile.");
            }
            catch { return (false, "Failed to update profile."); }
        }

        /// Changes the current user's password.
        public async Task<(bool ok, string? error)> ChangePasswordAsync(string current, string newPass)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/account/me/change-password",
                new { currentPassword = current, newPassword = newPass });
            if (response.IsSuccessStatusCode) return (true, null);
            try
            {
                var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, err?.Error ?? "Failed to change password.");
            }
            catch { return (false, "Failed to change password."); }
        }

        public record UserProfileDto(string Id, string Email, string DisplayName, string? AvatarUrl);
        private record ErrorResponse(string Error);

        // GET: Fetch all boards for the current user
        public async Task<List<BoardDto>?> GetBoardsAsync()
        {
            await SetAuthorizationHeader(); // ALWAYS call this first
            return await _httpClient.GetFromJsonAsync<List<BoardDto>>("api/boards");
        }

        // POST: Create a new board
        public async Task<BoardDto?> CreateBoardAsync(CreateBoardDto dto)
        {
            await SetAuthorizationHeader(); // ALWAYS call this first
            var response = await _httpClient.PostAsJsonAsync("api/boards", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BoardDto>();
            }
            return null;
        }

        // GET: Fetch a single board with its columns and tasks
        public async Task<BoardDto?> GetBoardAsync(Guid boardId)
        {
            await SetAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<BoardDto>($"api/boards/{boardId}");
        }

        // POST: Create a new column
        public async Task<KanbanColumnDto?> CreateColumnAsync(CreateColumnDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/columns", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<KanbanColumnDto>();
            }
            return null;
        }

        // PUT: Update a column (e.g. title)
        public async Task<bool> UpdateColumnAsync(Guid columnId, UpdateColumnDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/columns/{columnId}", dto);
            return response.IsSuccessStatusCode;
        }

        // POST: Create a new task
        public async Task<TaskItemDto?> CreateTaskAsync(CreateTaskDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/tasks", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskItemDto>();
            }
            return null;
        }

        // PATCH: Move a task to a different column
        public async Task<bool> MoveTaskAsync(Guid taskId, Guid targetColumnId, int newOrder)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PatchAsync($"api/tasks/{taskId}/move?targetColumnId={targetColumnId}&newOrder={newOrder}", null);
            return response.IsSuccessStatusCode;
        }

        // DELETE: Delete an entire column and its tasks
        public async Task<bool> DeleteColumnAsync(Guid columnId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/columns/{columnId}");
            return response.IsSuccessStatusCode;
        }

        // DELETE: Delete a specific task card
        public async Task<bool> DeleteTaskAsync(Guid taskId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}");
            return response.IsSuccessStatusCode;
        }
        
        // PUT: Update task title, description, priority, dueDate
        public async Task<bool> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task AssignDeveloperRoleAsync()
        {
            await SetAuthorizationHeader();
            await _httpClient.PostAsync("api/account/assign-developer-role", null);
        }

        public async Task<List<BoardDto>?> GetAllBoardsAsync()
        {
            var response = await _httpClient.GetAsync("api/boards/all");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<BoardDto>>();
            return new List<BoardDto>();
        }

        public async Task<bool> DeleteBoardAsync(Guid boardId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/boards/{boardId}");
            return response.IsSuccessStatusCode;
        }

        // PUT: Update a board
        public async Task<bool> UpdateBoardAsync(Guid boardId, UpdateBoardDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/boards/{boardId}", dto);
            return response.IsSuccessStatusCode;
        }

        // ── Teams ────────────────────────────────────────────────────
        public async Task<List<TeamDto>?> GetMyTeamsAsync()
        {
            await SetAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<TeamDto>>("api/teams");
        }

        public async Task<TeamDetailDto?> GetTeamAsync(Guid teamId)
        {
            await SetAuthorizationHeader();
            try { return await _httpClient.GetFromJsonAsync<TeamDetailDto>($"api/teams/{teamId}"); }
            catch { return null; }
        }

        public async Task<TeamDto?> CreateTeamAsync(CreateTeamDto dto)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/teams", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<TeamDto>();
            return null;
        }

        // Returns null on success, or error message string on failure
        public async Task<string?> AddTeamMemberAsync(Guid teamId, string email)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync($"api/teams/{teamId}/members", new { Email = email });
            if (response.IsSuccessStatusCode) return null;
            try
            {
                var err = await response.Content.ReadFromJsonAsync<ErrorDto>();
                return err?.Error ?? $"Error {(int)response.StatusCode}";
            }
            catch { return $"Server error ({(int)response.StatusCode})"; }
        }

        public async Task<bool> RemoveTeamMemberAsync(Guid teamId, string userId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/teams/{teamId}/members/{userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddBoardToTeamAsync(Guid teamId, Guid boardId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/teams/{teamId}/boards/{boardId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveBoardFromTeamAsync(Guid teamId, Guid boardId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/teams/{teamId}/boards/{boardId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTeamAsync(Guid teamId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/teams/{teamId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Guid?> GenerateInviteLinkAsync(Guid teamId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/teams/{teamId}/invite", null);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<InviteCodeDto>();
            return result?.InviteCode;
        }

        public async Task<bool> RevokeInviteLinkAsync(Guid teamId)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/teams/{teamId}/invite");
            return response.IsSuccessStatusCode;
        }

        public async Task<TeamInviteInfoDto?> GetInviteInfoAsync(Guid code)
        {
            try { return await _httpClient.GetFromJsonAsync<TeamInviteInfoDto>($"api/teams/join/{code}"); }
            catch { return null; }
        }

        public async Task<Guid?> JoinViaInviteCodeAsync(Guid code)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/teams/join/{code}", null);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<JoinResultDto>();
            return result?.TeamId;
        }

        // Helper DTOs for response deserialization
        private record ErrorDto(string? Error);
        private record InviteCodeDto(Guid InviteCode);
        private record JoinResultDto(Guid TeamId);
    }
}