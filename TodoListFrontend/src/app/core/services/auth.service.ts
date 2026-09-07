import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);
    private platformId = inject(PLATFORM_ID);
    private apiUrl = `${environment.apiUrl}/api/auth`;
    private accessToken: string | null = null;
    private restoreInFlight$: Observable<boolean> | null = null;

    login(data: LoginRequest) {
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data, { withCredentials: true }).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    register(data: RegisterRequest) {
        return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data, { withCredentials: true }).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    refreshToken() {
        return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, null, { withCredentials: true }).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    restoreSession(): Observable<boolean> {
        if (!isPlatformBrowser(this.platformId)) {
            return of(false);
        }

        if (this.accessToken) {
            return of(true);
        }

        if (!this.restoreInFlight$) {
            this.restoreInFlight$ = this.refreshToken().pipe(
                map(() => true),
                catchError(() => {
                    this.clearTokens();
                    return of(false);
                }),
                finalize(() => {
                    this.restoreInFlight$ = null;
                }),
                shareReplay(1)
            );
        }

        return this.restoreInFlight$;
    }

    logout() {
        return this.http.post(`${this.apiUrl}/logout`, null, { withCredentials: true }).pipe(
            finalize(() => {
                this.clearTokens();
                this.router.navigate(['/login']);
            })
        );
    }

    private saveTokens(response: AuthResponse): void {
        if (isPlatformBrowser(this.platformId)) {
            this.accessToken = response.accessToken;
        }
    }

    getAccessToken(): string | null {
        return isPlatformBrowser(this.platformId) ? this.accessToken : null;
    }

    clearTokens(): void {
        this.accessToken = null;
    }

    isLoggedIn(): boolean {
        return this.accessToken !== null;
    }
}
