import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, catchError, finalize, of, shareReplay, tap } from 'rxjs';
import { AuthService } from './auth.service';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';

export type AuthStatus = 'initializing' | 'authenticated' | 'anonymous';

@Injectable({ providedIn: 'root' })
export class AuthFacade {
  private readonly authService = inject(AuthService);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly statusState = signal<AuthStatus>('initializing');
  private initializeInFlight$: Observable<boolean> | null = null;

  readonly status = this.statusState.asReadonly();
  readonly isInitializing = computed(() => this.status() === 'initializing');
  readonly isAuthenticated = computed(() => this.status() === 'authenticated');

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      // Remove keys created by the old token-storage implementation once.
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      sessionStorage.removeItem('accessToken');
      sessionStorage.removeItem('refreshToken');
    }
  }

  initialize(): Observable<boolean> {
    if (!isPlatformBrowser(this.platformId)) {
      this.statusState.set('anonymous');
      return of(false);
    }

    if (this.status() === 'authenticated') {
      return of(true);
    }

    if (this.status() === 'anonymous') {
      return of(false);
    }

    if (!this.initializeInFlight$) {
      this.initializeInFlight$ = this.authService.restoreSession().pipe(
        tap(isAuthenticated => this.statusState.set(isAuthenticated ? 'authenticated' : 'anonymous')),
        catchError(() => {
          this.statusState.set('anonymous');
          return of(false);
        }),
        finalize(() => {
          this.initializeInFlight$ = null;
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }

    return this.initializeInFlight$;
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.authService.login(data).pipe(
      tap(() => this.statusState.set('authenticated'))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.authService.register(data).pipe(
      tap(() => this.statusState.set('authenticated'))
    );
  }

  logout() {
    return this.authService.logout().pipe(
      finalize(() => this.statusState.set('anonymous'))
    );
  }

  logoutAll() {
    return this.authService.logoutAll().pipe(
      finalize(() => this.statusState.set('anonymous'))
    );
  }

  markAnonymous(): void {
    this.authService.clearTokens();
    this.statusState.set('anonymous');
  }
}
