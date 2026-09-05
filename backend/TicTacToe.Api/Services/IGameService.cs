using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    Game Create(GameMode mode);
    Game Get(Guid id);
    Game MakeMove(Guid id, MakeMoveRequest request);
    Game Undo(Guid id);
    Game Reset(Guid id);
}
