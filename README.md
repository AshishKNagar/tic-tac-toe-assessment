# Tic Tac Toe – Angular + .NET 8 

Technical assessment solution .

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

  
AI-assisted development tools were used during the implementation of this assessment. 
All AI-generated suggestions and code were reviewed, adapted, and tested by me before 
being included in the final solution.

### AI Tools Used

- ChatGPT – used for brainstorming, implementation alternatives, debugging assistance,
  test-case suggestions, and code review support.
- GitHub Copilot – used for code completion and development productivity where appropriate.

### Example AI-Assisted Workflow

1. Converted the assessment requirements into a feature and acceptance checklist.
2. Used AI to explore implementation alternatives and identify potential edge cases.
3. Reviewed AI-generated suggestions against the assessment requirements and API contract.
4. Manually decided the architecture, API boundaries, validation rules, state ownership,
   and undo behavior.
5. Reviewed and adapted generated code to fit the project structure and coding standards.
6. Added and reviewed unit tests for valid moves, invalid moves, turn switching, win/draw
   detection, undo, reset, scoreboard behavior, and computer moves.
7. Ran the application and tests locally and investigated failures manually.
8. Refactored generated code where necessary rather than accepting AI output unchanged.

### What Was Generated vs. What Was Reviewed Manually

AI assistance was primarily used to accelerate development and explore implementation
options. The final implementation decisions remained my responsibility.

Examples of areas reviewed and/or modified manually:

- API contract and request/response models
- Backend game-state ownership
- Move validation and state transitions
- Undo semantics for Two Player and Computer modes
- Computer move-selection strategy
- Scoreboard update behavior
- Error handling
- Unit-test coverage and edge cases
- Angular-to-.NET API integration
- Project structure and maintainability

### Engineering Decisions

- **Backend is the source of truth** for game state, move validation, game status,
  move history, and scoreboard.
- **Domain/game logic is separated from controllers** so that business rules remain
  independently testable.
- **Computer strategy is deterministic and independently testable**.
- **Undo behavior is mode-specific**:
  - Two Player Mode: undo the most recent move.
  - Computer Mode: undo the computer move and the preceding human move together.
- **Completed games cannot be undone**. This keeps the completed result and scoreboard
  consistent and avoids reversing a finalized game.
- **Scoreboard is updated once per completed game** and is preserved when a game is reset.
- **In-memory storage** is used because persistence was not required for the assessment.
- **CI validates backend tests/build and frontend build** to catch integration and
  compilation issues before changes are merged.

### AI Usage Principle

AI was used as a development assistant, not as a substitute for engineering judgment.
The final code, design decisions, tests, and trade-offs were reviewed and validated by me.

## Design Trade-offs

### 1. In-Memory Storage vs Database

**Decision:** Use in-memory storage for this assessment.

**Why:**
- The assessment explicitly permits in-memory storage.
- The application is intended to run locally.
- It keeps the implementation simple and easy for the panel to run and review.

**Trade-off:**
- Game state is lost when the backend restarts.
- It does not support reliable state sharing across multiple API instances.

**Production improvement:**
Use Redis or a persistent database depending on scalability, durability and consistency requirements.

### 2. Backend State vs Frontend State

**Decision:** The backend is the source of truth.

**Why:**
- Centralizes game rules and move validation.
- Prevents the frontend from independently deciding game state.
- Makes state transitions easier to test consistently.

**Trade-off:**
- Game actions require REST API calls.
- There is additional network communication compared with keeping all game logic in the browser.

**Production benefit:**
Multiple clients can rely on the same authoritative game rules and state.

### 3. Undo After Game Completion

**Decision:** Disable Undo after a game is Won or Draw.

**Why:**
- Keeps the completed result final.
- Prevents the scoreboard from needing to be reversed.
- Reduces state-management complexity.

**Alternative:**
Allow Undo after completion and adjust the scoreboard when the result is reversed.

**Why it was not selected:**
The additional transactional/state complexity is unnecessary for this assessment.

### 4. Simple Computer Strategy vs Minimax

**Decision:** Implement the required deterministic priority-based strategy.

Priority:
1. Win if possible
2. Block X if X can win next
3. Take center
4. Take a corner
5. Take any available cell

**Why:**
- It directly satisfies the assessment requirement.
- It is deterministic and easy to understand.
- It is straightforward to unit test.

**Trade-off:**
The computer is not an optimal Tic Tac Toe player in every possible position.

**Future improvement:**
Use a minimax-based strategy if a stronger computer opponent is required.

### 5. Per-Game Synchronization vs Global Lock

**Decision:** Use a synchronization object per game.

**Why:**
- Concurrent operations on the same game are serialized.
- Unrelated games do not have to wait for each other.
- It provides atomic validation and state changes within a single API instance.

**Trade-off:**
An in-process lock does not coordinate state between multiple API instances.

**Production improvement:**
Use database optimistic concurrency/versioning, Redis, or distributed locking when horizontally scaling the API.

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
