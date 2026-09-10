import { PLATFORM_ID } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthFacade } from './auth.facade';
import { AuthService } from './auth.service';

describe('AuthFacade', () => {
  let authService: {
    restoreSession: ReturnType<typeof vi.fn>;
    clearTokens: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();
    authService = {
      restoreSession: vi.fn().mockReturnValue(of(true)),
      clearTokens: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        AuthFacade,
        { provide: AuthService, useValue: authService },
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    });
  });

  it('removes legacy browser token keys during initialization', () => {
    localStorage.setItem('accessToken', 'legacy-access');
    localStorage.setItem('refreshToken', 'legacy-refresh');
    sessionStorage.setItem('accessToken', 'legacy-session-access');
    sessionStorage.setItem('refreshToken', 'legacy-session-refresh');

    TestBed.inject(AuthFacade);

    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
    expect(sessionStorage.getItem('accessToken')).toBeNull();
    expect(sessionStorage.getItem('refreshToken')).toBeNull();
  });

  it('moves from initializing to authenticated after cookie refresh', () => {
    const facade = TestBed.inject(AuthFacade);

    expect(facade.status()).toBe('initializing');
    facade.initialize().subscribe();

    expect(authService.restoreSession).toHaveBeenCalledTimes(1);
    expect(facade.status()).toBe('authenticated');
  });
});
