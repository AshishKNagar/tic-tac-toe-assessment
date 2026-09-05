using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;

namespace TicTacToe.Api.Services;

public sealed class ScoreboardService : IScoreboardService
{
    private readonly object _sync = new();
    private int _xWins;
    private int _oWins;
    private int _draws;

    public ScoreboardDto Get()
    {
        lock (_sync)
        {
            return new ScoreboardDto(_xWins, _oWins, _draws);
        }
    }

    public void Record(GameStatus status, Player? winner)
    {
        lock (_sync)
        {
            if (status == GameStatus.Won && winner == Player.X) _xWins++;
            else if (status == GameStatus.Won && winner == Player.O) _oWins++;
            else if (status == GameStatus.Draw) _draws++;
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _xWins = 0;
            _oWins = 0;
            _draws = 0;
        }
    }
}
