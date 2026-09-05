using TicTacToe.Api.Domain;

namespace TicTacToe.Api.DTOs;
/// <summary>
///         
/// </summary>
/// <param name="Mode"></param>
public sealed record CreateGameRequest(GameMode Mode);

public sealed record MakeMoveRequest(
    Player Player,
    int Row,
    int Column);

public sealed record MoveDto(
    int MoveNumber,
    Player Player,
    int Row,
    int Column);

public sealed record ScoreboardDto(
    int XWins,
    int OWins,
    int Draws);

public sealed record GameStateDto(
    Guid GameId,
    string?[] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    int[] WinningCells,
    IReadOnlyList<MoveDto> MoveHistory,
    ScoreboardDto Scoreboard);
