using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameService _games;
    private readonly IScoreboardService _scoreboard;

    public GamesController(IGameService games, IScoreboardService scoreboard)
    {
        _games = games;
        _scoreboard = scoreboard;
    }

    /// <summary>
    ///     create games
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<GameStateDto> Create(CreateGameRequest request)
        => Ok(ToDto(_games.Create(request.Mode)));

    [HttpGet("{id:guid}")]
    public ActionResult<GameStateDto> Get(Guid id)
    {
        try { return Ok(ToDto(_games.Get(id))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>
    /// Move games
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateDto> Move(Guid id, MakeMoveRequest request)
    {
        try { return Ok(ToDto(_games.MakeMove(id, request))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentOutOfRangeException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
    /// <summary>
    /// Undo games
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateDto> Undo(Guid id)
    {
        try { return Ok(ToDto(_games.Undo(id))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    /// <summary>
    /// Reset games 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateDto> Reset(Guid id)
    {
        try { return Ok(ToDto(_games.Reset(id))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    private GameStateDto ToDto(Game game) =>
        new(
            game.Id,
            game.Board,
            game.CurrentPlayer,
            game.Mode,
            game.Status,
            game.Winner,
            game.WinningCells.ToArray(),
            game.Moves.Select(m => new MoveDto(m.MoveNumber, m.Player, m.Row, m.Column)).ToList(),
            _scoreboard.Get());
}
