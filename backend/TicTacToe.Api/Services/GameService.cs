using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;

namespace TicTacToe.Api.Services;

public sealed class GameService : IGameService
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    private readonly Dictionary<Guid, Game> _games = [];
    private readonly object _gamesSync = new();
    private readonly IScoreboardService _scoreboard;

    public GameService(IScoreboardService scoreboard) => _scoreboard = scoreboard;

    /// <summary>
    /// Create service
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public Game Create(GameMode mode)
    {
        var game = new Game { Mode = mode };
        lock (_gamesSync) _games[game.Id] = game;
        return game;
    }

    public Game Get(Guid id)
    {
        lock (_gamesSync)
        {
            if (!_games.TryGetValue(id, out var game))
                throw new KeyNotFoundException("Game was not found.");
            return game;
        }
    }

    public Game MakeMove(Guid id, MakeMoveRequest request)
    {
        var game = Get(id);

        lock (game.SyncRoot)
        {
            ValidateMove(game, request);

            ApplyMove(game, request.Player, request.Row, request.Column);
            EvaluateGame(game);

            if (game.Status == GameStatus.InProgress)
            {
                game.CurrentPlayer = Other(game.CurrentPlayer);

                if (game.Mode == GameMode.Computer && game.CurrentPlayer == Player.O)
                {
                    var index = SelectComputerMove(game);
                    ApplyMove(game, Player.O, index / 3, index % 3);
                    EvaluateGame(game);

                    if (game.Status == GameStatus.InProgress)
                        game.CurrentPlayer = Player.X;
                }
            }

            return game;
        }
    }

    public Game Undo(Guid id)
    {
        var game = Get(id);

        lock (game.SyncRoot)
        {
            if (game.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Undo is disabled after a game is completed.");

            if (game.Moves.Count == 0)
                throw new InvalidOperationException("There are no moves to undo.");

            var count = game.Mode == GameMode.Computer
                ? Math.Min(2, game.Moves.Count)
                : 1;

            for (var i = 0; i < count; i++)
            {
                var move = game.Moves[^1];
                game.Board[move.Row * 3 + move.Column] = null;
                game.Moves.RemoveAt(game.Moves.Count - 1);
            }

            game.CurrentPlayer = Player.X;
            if (game.Mode == GameMode.TwoPlayer && game.Moves.Count > 0)
                game.CurrentPlayer = Other(game.Moves[^1].Player);

            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells.Clear();

            return game;
        }
    }

    public Game Reset(Guid id)
    {
        var game = Get(id);

        lock (game.SyncRoot)
        {
            Array.Fill(game.Board, null);
            game.Moves.Clear();
            game.WinningCells.Clear();
            game.CurrentPlayer = Player.X;
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.ScoreRecorded = false;
            return game;
        }
    }

    private void ValidateMove(Game game, MakeMoveRequest request)
    {
        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("The game is already completed.");

        if (game.Mode == GameMode.Computer && request.Player != Player.X)
            throw new InvalidOperationException("Only Player X is controlled by the user in Computer mode.");

        if (request.Player != game.CurrentPlayer)
            throw new InvalidOperationException($"It is Player {game.CurrentPlayer}'s turn.");

        if (request.Row is < 0 or > 2 || request.Column is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(request), "Row and column must be between 0 and 2.");

        if (game.Board[request.Row * 3 + request.Column] is not null)
            throw new InvalidOperationException("The selected cell is already occupied.");
    }

    private static void ApplyMove(Game game, Player player, int row, int column)
    {
        var index = row * 3 + column;
        game.Board[index] = player.ToString();
        game.Moves.Add(new Move(game.Moves.Count + 1, player, row, column));
    }

    private void EvaluateGame(Game game)
    {
        foreach (var line in WinningLines)
        {
            var symbol = game.Board[line[0]];
            if (symbol is not null &&
                symbol == game.Board[line[1]] &&
                symbol == game.Board[line[2]])
            {
                game.Status = GameStatus.Won;
                game.Winner = symbol == "X" ? Player.X : Player.O;
                game.WinningCells.Clear();
                game.WinningCells.AddRange(line);
                RecordScoreOnce(game);
                return;
            }
        }

        if (game.Board.All(cell => cell is not null))
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells.Clear();
            RecordScoreOnce(game);
        }
    }

    private void RecordScoreOnce(Game game)
    {
        if (game.ScoreRecorded) return;
        _scoreboard.Record(game.Status, game.Winner);
        game.ScoreRecorded = true;
    }

    private static Player Other(Player player) => player == Player.X ? Player.O : Player.X;

    private static int SelectComputerMove(Game game)
    {
        var available = Enumerable.Range(0, 9)
            .Where(i => game.Board[i] is null)
            .ToList();

        var winningMove = FindWinningMove(game.Board, "O", available);
        if (winningMove >= 0) return winningMove;

        var blockingMove = FindWinningMove(game.Board, "X", available);
        if (blockingMove >= 0) return blockingMove;

        if (game.Board[4] is null) return 4;

        foreach (var corner in new[] { 0, 2, 6, 8 })
            if (game.Board[corner] is null) return corner;

        return available[0];
    }

    private static int FindWinningMove(string?[] board, string player, IEnumerable<int> available)
    {
        foreach (var index in available)
        {
            var copy = (string?[])board.Clone();
            copy[index] = player;

            if (WinningLines.Any(line =>
                copy[line[0]] == player &&
                copy[line[1]] == player &&
                copy[line[2]] == player))
                return index;
        }

        return -1;
    }
}
