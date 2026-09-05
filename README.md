# Tic Tac Toe – Angular + .NET 8

Technical assessment solution for the Software Development Manager role.

## Project Overview

A browser-based Tic Tac Toe application with:

- Angular + TypeScript frontend
- .NET 8 REST API backend
- Backend-owned game state
- Two Player mode
- Play Against Computer mode
- Move history
- Undo
- Scoreboard
- Reset Game and Reset Scoreboard
- xUnit backend tests
- GitHub Actions CI

## Architecture

The backend is the source of truth for game state, validation, game rules, move history and scoreboard.

```text
Angular UI
   |
   | REST/HTTP
   v
.NET Web API
   |
   +-- Controllers
   |
   +-- GameService
   |      +-- Game rules
   |      +-- Win/draw detection
   |      +-- Undo
   |      +-- Computer player
   |
   +-- ScoreboardService
   |
   +-- In-memory game sessions
```

The code intentionally keeps game rules outside controllers so the core behavior can be unit tested independently.

## Technology

- .NET 8
- ASP.NET Core Web API
- Angular 18
- TypeScript
- xUnit
- GitHub Actions
- In-memory storage

## Run Backend

Prerequisite: .NET 8 SDK.

```bash
cd backend
dotnet restore
dotnet run --project TicTacToe.Api
```

Swagger is available at:

```text
http://localhost:5000/swagger
```

If ASP.NET chooses another local port, use the URL printed by `dotnet run`.

## Run Frontend

Prerequisite: Node.js 20+ and npm.

```bash
cd frontend
npm install
npm start
```

Open the Angular URL printed by the CLI, normally:

```text
http://localhost:4200
```

The frontend expects the API at:

```text
http://localhost:5000/api
```

If your API runs on another port, update `frontend/src/app/game.service.ts`.

## API Contract

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create game |
| GET | `/api/games/{id}` | Get current game |
| POST | `/api/games/{id}/moves` | Submit move |
| POST | `/api/games/{id}/undo` | Undo |
| POST | `/api/games/{id}/reset` | Reset game |
| GET | `/api/scoreboard` | Get scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

### Create Game

```json
{
  "mode": "TwoPlayer"
}
```

or:

```json
{
  "mode": "Computer"
}
```

### Make Move

```json
{
  "player": "X",
  "row": 0,
  "column": 1
}
```

## Undo Decision

Undo is disabled after a game is Won or Draw.

Reason: this keeps a completed result final and avoids retroactively changing the session scoreboard.

In Two Player mode, Undo removes one move.

In Computer mode, Undo removes the human move and the computer response together.

## Computer Player Strategy

The computer follows the required priority:

1. Win if possible
2. Block X if X can win next
3. Take center
4. Take a corner
5. Take any available cell

## Testing

From the repository root:

```bash
dotnet test backend/TicTacToe.sln
```

Tests cover:

- Valid move
- Invalid occupied move
- Wrong player
- Row win
- Column win
- Diagonal win
- Draw
- Reset
- Two Player undo
- Computer mode
- Computer undo
- Scoreboard
- Move after completion

## AI-Assisted Development

AI tools may be used during development, but all generated code should be reviewed and tested by the candidate.

Example workflow:

- Convert the assessment requirements into a feature checklist.
- Ask AI for implementation alternatives.
- Review generated code against the requirements.
- Manually decide architecture, API boundaries, validation and undo semantics.
- Write/review unit tests for edge cases.
- Run the application and tests locally.
- Refactor generated code where appropriate.

The important engineering decisions in this solution are:

- Backend is the source of truth.
- Domain/game logic is separated from controllers.
- Computer strategy is deterministic and independently testable.
- Undo behavior is mode-specific.
- Scoreboard is updated once per completed game.
- Completed games cannot be undone.
- CI validates backend and frontend builds.

## Assumptions

- In-memory storage is sufficient for this local assessment.
- One game session is represented by one game ID.
- Authentication is outside the scope of the assessment.
- Computer is always Player O.
- Human is always Player X in Computer mode.
- Undo is disabled after completion.

## Known Limitations

- In-memory state is lost when the API process restarts.
- No authentication/authorization.
- No persistent database.
- CORS is intentionally permissive for local development.
- No multi-instance distributed state.

## Future Improvements

For production:

- SQL/Redis-backed game session storage
- Authentication and authorization
- Distributed locking/idempotency
- Centralized structured logging
- OpenTelemetry/Application Insights
- Rate limiting
- API versioning
- Integration tests
- Containerization
- Production CI/CD deployment
- Stronger computer AI such as minimax
