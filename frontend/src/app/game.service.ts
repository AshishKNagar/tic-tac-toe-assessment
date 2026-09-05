import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { GameMode, GameState, Player, Scoreboard } from './game.models';

/**
 * The .NET API exposes enums as numeric values in its Swagger contract:
 * Player: X=0, O=1
 * GameMode: TwoPlayer=0, Computer=1
 * GameStatus: InProgress=0, Won=1, Draw=2
 *
 * The UI uses readable string values, so this service translates the API
 * contract to the frontend model in one place.
 */
@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly api = 'http://localhost:5000/api';

  create(mode: GameMode) {
    return this.http.post<ApiGameState>(`${this.api}/games`, {
      // IMPORTANT: the backend Swagger contract expects GameMode as a number.
      mode: mode === 'Computer' ? 1 : 0
    }).pipe(map(state => this.toGameState(state)));
  }

  get(id: string) {
    return this.http.get<ApiGameState>(`${this.api}/games/${id}`)
      .pipe(map(state => this.toGameState(state)));
  }

  move(id: string, player: Player, row: number, column: number) {
    return this.http.post<ApiGameState>(`${this.api}/games/${id}/moves`, {
      // IMPORTANT: the backend Swagger contract expects Player as a number.
      player: player === 'O' ? 1 : 0,
      row,
      column
    }).pipe(map(state => this.toGameState(state)));
  }

  undo(id: string) {
    return this.http.post<ApiGameState>(`${this.api}/games/${id}/undo`, {})
      .pipe(map(state => this.toGameState(state)));
  }

  reset(id: string) {
    return this.http.post<ApiGameState>(`${this.api}/games/${id}/reset`, {})
      .pipe(map(state => this.toGameState(state)));
  }

  scoreboard() {
    return this.http.get<Scoreboard>(`${this.api}/scoreboard`);
  }

  resetScoreboard() {
    return this.http.post<Scoreboard>(`${this.api}/scoreboard/reset`, {});
  }

  private toGameState(api: ApiGameState): GameState {
    return {
      gameId: api.gameId,
      board: api.board ?? [],
      currentPlayer: this.toPlayer(api.currentPlayer),
      mode: this.toGameMode(api.mode),
      status: this.toGameStatus(api.status),
      winner: api.winner == null ? null : this.toPlayer(api.winner),
      winningCells: api.winningCells ?? [],
      moveHistory: (api.moveHistory ?? []).map(move => ({
        moveNumber: move.moveNumber,
        player: this.toPlayer(move.player),
        row: move.row,
        column: move.column
      })),
      scoreboard: api.scoreboard
    };
  }

  private toPlayer(value: number | string): Player {
    return value === 1 || value === 'O' ? 'O' : 'X';
  }

  private toGameMode(value: number | string): GameMode {
    return value === 1 || value === 'Computer' ? 'Computer' : 'TwoPlayer';
  }

  private toGameStatus(value: number | string): 'InProgress' | 'Won' | 'Draw' {
    if (value === 1 || value === 'Won') return 'Won';
    if (value === 2 || value === 'Draw') return 'Draw';
    return 'InProgress';
  }
}

interface ApiMove {
  moveNumber: number;
  player: number | string;
  row: number;
  column: number;
}

interface ApiGameState {
  gameId: string;
  board: (string | null)[];
  currentPlayer: number | string;
  mode: number | string;
  status: number | string;
  winner: number | string | null;
  winningCells: number[];
  moveHistory: ApiMove[];
  scoreboard: Scoreboard;
}
