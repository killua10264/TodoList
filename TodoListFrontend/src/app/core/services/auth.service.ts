import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse, TokenRequest } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);
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
        return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
            tap(() => {
                this.clearTokens();
                this.router.navigate(['/login']);
            })
        );
    }

    //help function
    private saveTokens(response: AuthResponse): void {
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
    }

    getAccessToken(): string | null {
        return localStorage.getItem('accessToken');
    }

    getRefreshToken(): string | null {
        return localStorage.getItem('refreshToken');
    }

    private clearTokens(): void {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }

    isLoggedIn(): boolean {
        return !!this.getAccessToken();
    }
}
