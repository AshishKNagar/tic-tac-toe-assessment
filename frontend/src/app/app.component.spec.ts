import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the application', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const request = httpMock.expectOne('http://localhost:5000/api/games');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ mode: 'TwoPlayer' });
    request.flush({
      gameId: 'test-game-id',
      board: [null, null, null, null, null, null, null, null, null],
      currentPlayer: 'X',
      mode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [],
      scoreboard: { xWins: 0, oWins: 0, draws: 0 }
    });
    expect(fixture.componentInstance).toBeTruthy();
  });
});
