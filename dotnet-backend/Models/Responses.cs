using System.Text.Json.Serialization;
using DotnetBackend.Models;

namespace DotnetBackend.Models;

public class UsersResponse
{
    [JsonPropertyName("users")]
    public List<User> Users { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class TasksResponse
{
    [JsonPropertyName("tasks")]
    public List<TaskItem> Tasks { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class UsersStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class TasksStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("pending")]
    public int Pending { get; set; }

    [JsonPropertyName("inProgress")]
    public int InProgress { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }
}

public class StatsResponse
{
    [JsonPropertyName("users")]
    public UsersStats Users { get; set; } = new();

    [JsonPropertyName("tasks")]
    public TasksStats Tasks { get; set; } = new();
}

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>Request body for POST /api/users</summary>
public class CreateUserRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

/// <summary>Request body for POST /api/tasks</summary>
public class CreateTaskRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("userId")]
    public int? UserId { get; set; }
}

/// <summary>Request body for PUT /api/tasks/{id} — all fields optional for partial updates</summary>
public class UpdateTaskRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("userId")]
    public int? UserId { get; set; }
}
