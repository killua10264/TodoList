import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
    TodoResponse, TodoCreateRequest, TodoUpdateRequest, PaginatedResponse
} from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/todos`;

    getAll(page: number = 1, pageSize: number = 20) {
        const params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        return this.http.get<PaginatedResponse<TodoResponse>>(this.apiUrl, { params });
    }

    create(data: TodoCreateRequest) {
        return this.http.post<TodoResponse>(this.apiUrl, data);
    }

    update(id: number, data: TodoUpdateRequest) {
        return this.http.put<TodoResponse>(`${this.apiUrl}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
