import { Injectable, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { tap, EMPTY } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserResponse, UserUpdateRequest, ChangePasswordRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/users`;
    private platformId = inject(PLATFORM_ID);
    currentUser = signal<UserResponse | null>(null);

    getProfile() {
        if (!isPlatformBrowser(this.platformId)) {
            return EMPTY;
        }
        return this.http.get<UserResponse>(`${this.apiUrl}/profile`).pipe(
            tap(user => this.currentUser.set(user))
        );
    }

    updateProfile(data: UserUpdateRequest) {
        return this.http.put<{ message: string; data: UserResponse }>(`${this.apiUrl}/profile`, data).pipe(
            tap(res => {
                if (res.data) {
                    this.currentUser.set(res.data);
                }
            })
        );
    }

    uploadAvatar(file: File) {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ message: string; avatarUrl: string; data: UserResponse }>(`${this.apiUrl}/profile/avatar`, formData).pipe(
            tap(res => {
                if (res.data) {
                    this.currentUser.set(res.data);
                }
            })
        );
    }

    changePassword(data: ChangePasswordRequest) {
        return this.http.put(`${this.apiUrl}/change-password`, data);
    }
}

