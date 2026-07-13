import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  SubTaskResponse, SubTaskCreateDto, SubTaskUpdateDto
} from '../models/subtask.model';

@Injectable({ providedIn: 'root' })
export class SubTaskService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/subtasks`;

  refresh$ = new Subject<void>();

  notifyChanged() {
    this.refresh$.next();
  }

  getByTodoId(todoId: number) {
    return this.http.get<SubTaskResponse[]>(`${this.apiUrl}/by-todo/${todoId}`);
  }

  create(data: SubTaskCreateDto) {
    return this.http.post<SubTaskResponse>(this.apiUrl, data).pipe(
      tap(() => this.notifyChanged())
    );
  }

  update(id: number, data: SubTaskUpdateDto) {
    return this.http.put<SubTaskResponse>(`${this.apiUrl}/${id}`, data).pipe(
      tap(() => this.notifyChanged())
    );
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.notifyChanged())
    );
  }
}
