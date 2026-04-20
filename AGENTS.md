# Project Guidelines

This is a 3-tier developer assessment project. Candidates must implement missing endpoints in the .NET backend. See [TEST_REQUIREMENTS.md](TEST_REQUIREMENTS.md) for the full task list and [GETTING_STARTED.md](GETTING_STARTED.md) for setup steps.

## Architecture

```
React (Vite, :5173) → Node.js/Express (:3000) → ASP.NET Core Minimal API (:8080)
```

- React proxies all `/api` requests to Node.js via Vite's dev server.
- Node.js is a thin proxy — it forwards requests to .NET using the raw `http` module (`node-backend/utils/httpClient.js`).
- **All data lives in .NET**: `dotnet-backend/Data/DataStore.cs` (in-memory, resets on restart).
- The Node.js config key for the .NET backend URL is `GO_BACKEND_URL` (legacy name — do **not** rename it).

## Build & Run

Start all three services (each in its own terminal, in this order):

```bash
# 1. .NET backend
cd dotnet-backend && dotnet restore && dotnet run

# 2. Node.js backend
cd node-backend && npm install && npm start

# 3. React frontend
cd react-frontend && npm install && npm run dev
```

## .NET Backend Conventions

- **Minimal API** — all routes are registered directly in `Program.cs`. Do not create controllers.
- **DataStore** is a `Singleton`; use `EnterWriteLock`/`ExitWriteLock` (from `ReaderWriterLockSlim`) for all write operations, matching the existing pattern.
- **Response DTOs** are in `Models/Responses.cs` with `[JsonPropertyName]` for camelCase JSON output.
- **Task status values** are exact strings: `"pending"`, `"in-progress"`, `"completed"`.
- **ID generation**: `max existing ID + 1`.
- **Partial updates**: `PUT /api/tasks/{id}` must update only fields that are provided (non-null).
- Port is read from the `PORT` env var, defaulting to `8080`.

## Missing Endpoints (Assessment Tasks)

The Node.js controllers and React forms are already wired up. Only the .NET side needs implementation:

| Method | Path | Notes |
|--------|------|-------|
| `POST` | `/api/users` | Create user; validate Name, Email, Role |
| `POST` | `/api/tasks` | Create task; validate Title, UserId exists |
| `PUT`  | `/api/tasks/{id}` | Partial update of Title, Status, UserId |

See [TEST_REQUIREMENTS.md](TEST_REQUIREMENTS.md) for exact validation rules and expected responses.

## Node.js Backend Conventions

- Routes in `routes/`, handlers in `controllers/`. Do not add business logic — it is a proxy only.
- Error propagation: match `.statusCode` from `httpClient` errors and forward the same code to the client.
- Unused packages in `package.json` (`mongoose`, `jsonwebtoken`, etc.) are intentional leftovers — ignore them.

## React Frontend Conventions

- `App.jsx` owns all state; child components are display-only.
- All API calls go through `src/services/api.js` (`apiClient` axios instance).
- After any mutation, reload all data with `Promise.all([getUsers(), getTasks(), getStats()])`.
- Do **not** call the .NET backend directly from the frontend — always go through Node.js.

## Environment Variables

| Variable | Component | Default |
|----------|-----------|---------|
| `PORT` | .NET backend | `8080` |
| `PORT` | Node.js | `3000` |
| `GO_BACKEND_URL` | Node.js | `http://localhost:8080` |
| `VITE_API_URL` | React | `http://localhost:3000` |

No `.env` files exist — defaults are in `node-backend/config/index.js` and `react-frontend/vite.config.js`.
