import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserUpdateRequest, ChangePasswordRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/users`;

    updateProfile(data: UserUpdateRequest) {
        return this.http.put(`${this.apiUrl}/profile`, data);
    }

    changePassword(data: ChangePasswordRequest) {
        return this.http.put(`${this.apiUrl}/change-password`, data);
    }
}
