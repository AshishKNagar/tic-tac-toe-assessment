using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public sealed class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboard;

    public ScoreboardController(IScoreboardService scoreboard) => _scoreboard = scoreboard;

    [HttpGet]
    public ActionResult<ScoreboardDto> Get() => Ok(_scoreboard.Get());

    /// <summary>
    /// Reset scoreboard
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset")]
    public ActionResult<ScoreboardDto> Reset()
    {
        _scoreboard.Reset();
        return Ok(_scoreboard.Get());
    }
}
