using System.Net.Http.Json;
using System.Xml;
using TaskTracker.Models;
using Microsoft.Extensions.Options;

namespace TaskTracker.Services
{
    public class ClockifyRemoteService : IRemoteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _workspaceId;

        public ClockifyRemoteService(IHttpClientFactory httpClientFactory, IOptions<ClockifySettings> clockifyOptions)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = clockifyOptions.Value.ApiKey;
            _workspaceId = clockifyOptions.Value.WorkspaceId;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            return client;
        }

        public async Task<string> CreateProjectAsync(string name)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/projects";
            var payload = new { name, isPublic = true };

            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Clockify API error creating project: {error}", null, response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<ClockifyProjectResponse>();
            if (result == null || string.IsNullOrEmpty(result.Id))
            {
                throw new InvalidOperationException("Failed to parse project ID from Clockify response.");
            }

            return result.Id;
        }

        public async Task UpdateProjectAsync(string remoteProjectId, string name)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/projects/{remoteProjectId}";
            var payload = new { name, isPublic = true };

            var response = await client.PutAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Clockify API error updating project: {error}", null, response.StatusCode);
            }
        }

        public async Task<List<RemoteUserDto>> GetUsersAsync()
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/users";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Clockify API error getting users: {error}", null, response.StatusCode);
            }

            var users = await response.Content.ReadFromJsonAsync<List<ClockifyUserResponse>>();
            if (users == null)
            {
                return new List<RemoteUserDto>();
            }

            return users
                .Select(u => new RemoteUserDto(u.Id, u.Name))
                .ToList();
        }

        public async Task<string> CreateTaskAsync(string remoteProjectId, string taskName, string remoteUserId, decimal estimatedHours)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/projects/{remoteProjectId}/tasks";
            var estimateIso = FormatIsoDuration(estimatedHours);

            var payload = new
            {
                name = taskName,
                assigneeIds = new[] { remoteUserId },
                estimate = estimateIso
            };

            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                if (error.Contains("Assignee is not a member of project"))
                {
                    // Auto-make project public and retry
                    await EnsureProjectPublicAsync(client, remoteProjectId);
                    response = await client.PostAsJsonAsync(url, payload);
                }

                if (!response.IsSuccessStatusCode)
                {
                    error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Clockify API error creating task: {error}", null, response.StatusCode);
                }
            }

            var result = await response.Content.ReadFromJsonAsync<ClockifyTaskResponse>();
            if (result == null || string.IsNullOrEmpty(result.Id))
            {
                throw new InvalidOperationException("Failed to parse task ID from Clockify response.");
            }

            return result.Id;
        }

        public async Task UpdateTaskAsync(string remoteProjectId, string remoteTaskId, string taskName, string remoteUserId, decimal estimatedHours)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/projects/{remoteProjectId}/tasks/{remoteTaskId}";
            var estimateIso = FormatIsoDuration(estimatedHours);

            var payload = new
            {
                name = taskName,
                assigneeIds = new[] { remoteUserId },
                estimate = estimateIso
            };

            var response = await client.PutAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                if (error.Contains("Assignee is not a member of project"))
                {
                    // Auto-make project public and retry
                    await EnsureProjectPublicAsync(client, remoteProjectId);
                    response = await client.PutAsJsonAsync(url, payload);
                }

                if (!response.IsSuccessStatusCode)
                {
                    error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Clockify API error updating task: {error}", null, response.StatusCode);
                }
            }
        }

        public async Task<string> CreateTimeEntryAsync(string remoteProjectId, string remoteTaskId, string description, DateTime start, DateTime? end)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/time-entries";

            var payload = new
            {
                start = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                end = end.HasValue ? end.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
                projectId = remoteProjectId,
                taskId = remoteTaskId,
                description
            };

            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Clockify API error creating time entry: {error}", null, response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<ClockifyTimeEntryResponse>();
            if (result == null || string.IsNullOrEmpty(result.Id))
            {
                throw new InvalidOperationException("Failed to parse time entry ID from Clockify response.");
            }

            return result.Id;
        }

        public async Task UpdateTimeEntryAsync(string remoteTimeEntryId, string remoteProjectId, string remoteTaskId, string description, DateTime start, DateTime? end)
        {
            var client = CreateClient();
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/time-entries/{remoteTimeEntryId}";

            var payload = new
            {
                start = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                end = end.HasValue ? end.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
                projectId = remoteProjectId,
                taskId = remoteTaskId,
                description
            };

            var response = await client.PutAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Clockify API error updating time entry: {error}", null, response.StatusCode);
            }
        }

        private static string? FormatIsoDuration(decimal hours)
        {
            if (hours <= 0) return null;
            var timeSpan = TimeSpan.FromMinutes(Math.Round((double)hours * 60));
            return XmlConvert.ToString(timeSpan);
        }

        private async Task EnsureProjectPublicAsync(HttpClient client, string remoteProjectId)
        {
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/projects/{remoteProjectId}";
            var payload = new { isPublic = true };
            await client.PutAsJsonAsync(url, payload);
        }
    }
}
