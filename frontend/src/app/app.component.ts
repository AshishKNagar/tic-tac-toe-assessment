import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GameService } from './game.service';
import { GameMode, GameState, Player } from './game.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private readonly gameService = inject(GameService);

  game: GameState | null = null;
  selectedMode: GameMode = 'TwoPlayer';
  error = '';
  loading = false;

  constructor() {
    this.startNewGame();
  }

  startNewGame(): void {
    this.error = '';
    this.loading = true;
    this.gameService.create(this.selectedMode).subscribe({
      next: game => {
        this.game = game;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.showError(err);
      }
    });
  }

  changeMode(): void {
    this.startNewGame();
  }

  play(index: number): void {
    if (!this.game || this.game.status !== 'InProgress' || this.game.board[index]) {
      return;
    }

    const row = Math.floor(index / 3);
    const column = index % 3;

    this.error = '';
    this.gameService.move(
      this.game.gameId,
      this.game.currentPlayer,
      row,
      column
    ).subscribe({
      next: game => this.game = game,
      error: err => this.showError(err)
    });
  }

  undo(): void {
    if (!this.game || this.game.moveHistory.length === 0 || this.game.status !== 'InProgress') {
      return;
    }

    this.error = '';
    this.gameService.undo(this.game.gameId).subscribe({
      next: game => this.game = game,
      error: err => this.showError(err)
    });
  }

  resetGame(): void {
    if (!this.game) return;

    this.error = '';
    this.gameService.reset(this.game.gameId).subscribe({
      next: game => this.game = game,
      error: err => this.showError(err)
    });
  }

  resetScoreboard(): void {
    this.gameService.resetScoreboard().subscribe({
      next: score => {
        if (this.game) {
          this.game = { ...this.game, scoreboard: score };
        }
      },
      error: err => this.showError(err)
    });
  }

  cellLabel(index: number): string {
    return `Row ${Math.floor(index / 3) + 1}, Column ${(index % 3) + 1}`;
  }

  statusMessage(): string {
    if (!this.game) return '';
    if (this.game.status === 'Won') return `Player ${this.game.winner} wins!`;
    if (this.game.status === 'Draw') return 'Game drawn!';
    return `Player ${this.game.currentPlayer}'s turn`;
  }

  private showError(error: any): void {
    console.error('API error:', error);

    if (typeof error?.error === 'string' && error.error.trim()) {
      this.error = error.error;
      return;
    }

    this.error = error?.error?.message
      ?? error?.message
      ?? `Request failed${error?.status ? ` (HTTP ${error.status})` : ''}.`;
  }
}
