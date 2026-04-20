using DotnetBackend.Data;
using DotnetBackend.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataStore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// ── Request logging middleware ─────────────────────────────────────────────
// Logs every request: method, path, status code, and elapsed time.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("RequestLogger");

    var sw = Stopwatch.StartNew();
    await next(context);
    sw.Stop();

    logger.LogInformation(
        "{Method} {Path} → {StatusCode} ({Elapsed}ms)",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds);
});
// ──────────────────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

const int defaultPort = 8080;
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(portEnv, out var port))
{
    port = defaultPort;
}

// Valid task status values
string[] ValidStatuses = ["pending", "in-progress", "completed"];

app.MapGet("/health", () =>
{
    return Results.Json(new HealthResponse
    {
        Status = "ok",
        Message = ".NET backend is running"
    });
});

app.MapGet("/api/users", (DataStore store) =>
{
    var users = store.GetUsers();
    var response = new UsersResponse
    {
        Users = users,
        Count = users.Count
    };
    return Results.Json(response);
});

app.MapGet("/api/users/{id:int}", (int id, DataStore store) =>
{
    var user = store.GetUserById(id);
    return user is null
        ? Results.NotFound(new { error = "User not found" })
        : Results.Json(user);
});

// POST /api/users — create a new user
app.MapPost("/api/users", (CreateUserRequest req, DataStore store) =>
{
    // Validate required fields
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "Name is required" });

    if (string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new { error = "Email is required" });

    if (string.IsNullOrWhiteSpace(req.Role))
        return Results.BadRequest(new { error = "Role is required" });

    // Basic email format validation
    if (!Regex.IsMatch(req.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        return Results.BadRequest(new { error = "Invalid email format" });

    var user = store.CreateUser(req.Name.Trim(), req.Email.Trim(), req.Role.Trim());
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapGet("/api/tasks", (string? status, string? userId, DataStore store) =>
{
    var tasks = store.GetTasks(status, userId);
    var response = new TasksResponse
    {
        Tasks = tasks,
        Count = tasks.Count
    };
    return Results.Json(response);
});

// POST /api/tasks — create a new task
app.MapPost("/api/tasks", (CreateTaskRequest req, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    if (string.IsNullOrWhiteSpace(req.Status))
        return Results.BadRequest(new { error = "Status is required" });

    if (!ValidStatuses.Contains(req.Status))
        return Results.BadRequest(new { error = $"Status must be one of: {string.Join(", ", ValidStatuses)}" });

    if (req.UserId is null)
        return Results.BadRequest(new { error = "UserId is required" });

    // Verify the referenced user exists
    if (store.GetUserById(req.UserId.Value) is null)
        return Results.BadRequest(new { error = $"User with id {req.UserId} does not exist" });

    var task = store.CreateTask(req.Title.Trim(), req.Status, req.UserId.Value);
    return Results.Created($"/api/tasks/{task.Id}", task);
});

// PUT /api/tasks/{id} — partial update of an existing task
app.MapPut("/api/tasks/{id:int}", (int id, UpdateTaskRequest req, DataStore store) =>
{
    // Validate status only if provided
    if (req.Status is not null && !ValidStatuses.Contains(req.Status))
        return Results.BadRequest(new { error = $"Status must be one of: {string.Join(", ", ValidStatuses)}" });

    // Validate userId only if provided
    if (req.UserId is not null && store.GetUserById(req.UserId.Value) is null)
        return Results.BadRequest(new { error = $"User with id {req.UserId} does not exist" });

    var updated = store.UpdateTask(id, req.Title?.Trim(), req.Status, req.UserId);
    return updated is null
        ? Results.NotFound(new { error = $"Task with id {id} not found" })
        : Results.Json(updated);
});

app.MapGet("/api/stats", (DataStore store) =>
{
    var stats = store.GetStats();
    return Results.Json(stats);
});

app.Run($"http://0.0.0.0:{port}");
