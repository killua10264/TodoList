import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    CategoryResponse, CategoryCreateRequest, CategoryUpdateRequest
} from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/categories`;

    refresh$ = new Subject<void>();

    notifyChanged() {
        this.refresh$.next();
    }

    getAll() {
        return this.http.get<CategoryResponse[]>(this.apiUrl);
    }

    getById(id: number) {
        return this.http.get<CategoryResponse>(`${this.apiUrl}/${id}`);
    }

    create(data: CategoryCreateRequest) {
        return this.http.post<CategoryResponse>(this.apiUrl, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    update(id: number, data: CategoryUpdateRequest) {
        return this.http.put<CategoryResponse>(`${this.apiUrl}/${id}`, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    delete(id: number) {
        return this.http.delete(`${this.apiUrl}/${id}`).pipe(
            tap(() => this.notifyChanged())
        );
    }
}
