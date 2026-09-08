import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    TodoResponse, TodoCreateRequest, TodoUpdateRequest, PaginatedResponse
} from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/todos`;

    refresh$ = new Subject<void>();
    todoUpdated$ = new Subject<TodoResponse>();

    notifyChanged() {
        this.refresh$.next();
    }

    getAll(page: number = 1, pageSize: number = 20, filter?: string | null, categoryId?: number | null, status?: string | null, sortBy?: string | null, isHidden?: boolean | null, search?: string | null, isDeleted?: boolean | null) {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (filter) params = params.set('filter', filter);
        if (categoryId && categoryId > 0) params = params.set('categoryId', categoryId.toString());
        if (status && status !== 'all') params = params.set('status', status);
        if (sortBy) params = params.set('sortBy', sortBy);
        if (isHidden !== null && isHidden !== undefined) params = params.set('isHidden', isHidden.toString());
        if (search && search.trim()) params = params.set('search', search.trim());
        if (isDeleted !== null && isDeleted !== undefined) params = params.set('isDeleted', isDeleted.toString());

        return this.http.get<PaginatedResponse<TodoResponse>>(this.apiUrl, { params });
    }

    getById(id: number) {
        return this.http.get<TodoResponse>(`${this.apiUrl}/${id}`);
    }

    create(data: TodoCreateRequest) {
        return this.http.post<TodoResponse>(this.apiUrl, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    update(id: number, data: TodoUpdateRequest) {
        return this.http.put<TodoResponse>(`${this.apiUrl}/${id}`, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    updateSilent(id: number, data: TodoUpdateRequest) {
        return this.http.put<TodoResponse>(`${this.apiUrl}/${id}`, data);
    }

    delete(id: number, version: number) {
        const url = `${this.apiUrl}/${id}?version=${version}`;
        return this.http.delete(url).pipe(
            tap(() => this.notifyChanged())
        );
    }

    restore(id: number, version: number) {
        const url = `${this.apiUrl}/${id}/restore?version=${version}`;
        return this.http.post(url, {}).pipe(
            tap(() => this.notifyChanged())
        );
    }

    hardDelete(id: number, version: number) {
        const url = `${this.apiUrl}/${id}/hard?version=${version}`;
        return this.http.delete(url).pipe(
            tap(() => this.notifyChanged())
        );
    }
}
