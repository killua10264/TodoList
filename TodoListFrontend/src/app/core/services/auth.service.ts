import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse, TokenRequest } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);
    private platformId = inject(PLATFORM_ID);
    private apiUrl = `${environment.apiUrl}/api/auth`;

    login(data: LoginRequest) {
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    register(data: RegisterRequest) {
        return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    refreshToken() {
        const refreshToken = this.getRefreshToken();
        return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`,
            { refreshToken } as TokenRequest
        ).pipe(
            tap(response => this.saveTokens(response))
        );
    }

    logout() {
        this.clearTokens();
        return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
            tap(() => {
                this.router.navigate(['/login']);
            })
        );
    }

    // Help functions - an toàn với SSR (Server-Side Rendering)
    private saveTokens(response: AuthResponse): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('accessToken', response.accessToken);
            localStorage.setItem('refreshToken', response.refreshToken);
        }
    }

    getAccessToken(): string | null {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem('accessToken');
        }
        return null;
    }

    getRefreshToken(): string | null {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem('refreshToken');
        }
        return null;
    }

    clearTokens(): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
        }
    }

    isLoggedIn(): boolean {
        return !!this.getAccessToken();
    }
}
