namespace TicTacToe.Api.Domain;

public sealed class Game
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string?[] Board { get; } = new string?[9];
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode Mode { get; init; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; } = [];
    public List<Move> Moves { get; } = [];
    public bool ScoreRecorded { get; set; }
    public object SyncRoot { get; } = new();
}
