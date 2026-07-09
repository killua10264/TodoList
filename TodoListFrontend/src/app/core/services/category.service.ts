import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
    CategoryResponse, CategoryCreateRequest, CategoryUpdateRequest
} from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/categories`;

    getAll() {
        return this.http.get<CategoryResponse[]>(this.apiUrl);
    }

    create(data: CategoryCreateRequest) {
        return this.http.post<CategoryResponse>(this.apiUrl, data);
    }

    update(id: number, data: CategoryUpdateRequest) {
        return this.http.put<CategoryResponse>(`${this.apiUrl}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
