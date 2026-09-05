using System;
using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public sealed class GameServiceTests
{
    private static (GameService Game, ScoreboardService Score) CreateService()
    {
        var score = new ScoreboardService();
        return (new GameService(score), score);
    }

    [Fact]
    public void ValidMove_ShouldPlaceMarkAndSwitchTurn()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new MakeMoveRequest(Player.X, 0, 0));

        Assert.Equal("X", game.Board[0]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Single(game.Moves);
    }

    [Fact]
    public void InvalidOccupiedMove_ShouldBeRejected()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new MakeMoveRequest(Player.X, 0, 0));

        Assert.Throws<InvalidOperationException>(() =>
            service.MakeMove(game.Id, new MakeMoveRequest(Player.O, 0, 0)));
    }

    [Fact]
    public void WrongPlayer_ShouldBeRejected()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        Assert.Throws<InvalidOperationException>(() =>
            service.MakeMove(game.Id, new MakeMoveRequest(Player.O, 0, 0)));
    }

    [Fact]
    public void RowWin_ShouldUpdateWinnerAndScoreboard()
    {
        var (service, score) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 1, 0));
        service.MakeMove(game.Id, new(Player.X, 0, 1));
        service.MakeMove(game.Id, new(Player.O, 1, 1));
        service.MakeMove(game.Id, new(Player.X, 0, 2));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal([0, 1, 2], game.WinningCells);
        Assert.Equal(1, score.Get().XWins);
    }

    [Fact]
    public void ColumnWin_ShouldBeDetected()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 0, 1));
        service.MakeMove(game.Id, new(Player.X, 1, 0));
        service.MakeMove(game.Id, new(Player.O, 1, 1));
        service.MakeMove(game.Id, new(Player.X, 2, 0));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal([0, 3, 6], game.WinningCells);
    }

    [Fact]
    public void DiagonalWin_ShouldBeDetected()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 0, 1));
        service.MakeMove(game.Id, new(Player.X, 1, 1));
        service.MakeMove(game.Id, new(Player.O, 0, 2));
        service.MakeMove(game.Id, new(Player.X, 2, 2));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal([0, 4, 8], game.WinningCells);
    }

    [Fact]
    public void Draw_ShouldUpdateScoreboard()
    {
        var (service, score) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        var moves = new[]
        {
            new MakeMoveRequest(Player.X, 0, 0),
            new MakeMoveRequest(Player.O, 0, 1),
            new MakeMoveRequest(Player.X, 0, 2),
            new MakeMoveRequest(Player.O, 1, 1),
            new MakeMoveRequest(Player.X, 1, 0),
            new MakeMoveRequest(Player.O, 1, 2),
            new MakeMoveRequest(Player.X, 2, 1),
            new MakeMoveRequest(Player.O, 2, 0),
            new MakeMoveRequest(Player.X, 2, 2)
        };

        foreach (var move in moves) service.MakeMove(game.Id, move);

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Equal(1, score.Get().Draws);
    }

    [Fact]
    public void Reset_ShouldClearBoardAndHistoryAndKeepScore()
    {
        var (service, score) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 1, 0));
        service.MakeMove(game.Id, new(Player.X, 0, 1));
        service.MakeMove(game.Id, new(Player.O, 1, 1));
        service.MakeMove(game.Id, new(Player.X, 0, 2));

        Assert.Equal(1, score.Get().XWins);

        service.Reset(game.Id);

        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Empty(game.Moves);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(1, score.Get().XWins);
    }

    [Fact]
    public void Undo_TwoPlayer_ShouldRemoveOneMove()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 1, 1));

        service.Undo(game.Id);

        Assert.Null(game.Board[4]);
        Assert.Single(game.Moves);
        Assert.Equal(Player.O, game.CurrentPlayer);
    }

    [Fact]
    public void ComputerMode_ShouldMakeComputerMoveAutomatically()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.Computer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));

        Assert.Equal(2, game.Moves.Count);
        Assert.Equal("O", game.Board[4]); // center is the preferred move
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void ComputerMode_Undo_ShouldRemoveHumanAndComputerMoves()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.Computer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.Undo(game.Id);

        Assert.Empty(game.Moves);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void MoveAfterCompletion_ShouldBeRejected()
    {
        var (service, _) = CreateService();
        var game = service.Create(GameMode.TwoPlayer);

        service.MakeMove(game.Id, new(Player.X, 0, 0));
        service.MakeMove(game.Id, new(Player.O, 1, 0));
        service.MakeMove(game.Id, new(Player.X, 0, 1));
        service.MakeMove(game.Id, new(Player.O, 1, 1));
        service.MakeMove(game.Id, new(Player.X, 0, 2));

        Assert.Throws<InvalidOperationException>(() =>
            service.MakeMove(game.Id, new(Player.O, 2, 2)));
    }
}
