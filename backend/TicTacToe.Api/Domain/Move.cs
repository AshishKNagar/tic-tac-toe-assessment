namespace TicTacToe.Api.Domain;

public sealed record Move(
    int MoveNumber,
    Player Player,
    int Row,
    int Column);
