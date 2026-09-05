using TicTacToe.Api.DTOs;
using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    ScoreboardDto Get();
    void Record(GameStatus status, Player? winner);
    void Reset();
}
