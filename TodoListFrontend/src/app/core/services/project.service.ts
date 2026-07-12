import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    ProjectResponse, ProjectCreateRequest, ProjectUpdateRequest
} from '../models/project.model';

@Injectable({ providedIn: 'root' })
export class ProjectService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/projects`;

    refresh$ = new Subject<void>();

    notifyChanged() {
        this.refresh$.next();
    }

    getAll() {
        return this.http.get<ProjectResponse[]>(this.apiUrl);
    }

    getById(id: number) {
        return this.http.get<ProjectResponse>(`${this.apiUrl}/${id}`);
    }

    create(data: ProjectCreateRequest) {
        return this.http.post<ProjectResponse>(this.apiUrl, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    update(id: number, data: ProjectUpdateRequest) {
        return this.http.put<ProjectResponse>(`${this.apiUrl}/${id}`, data).pipe(
            tap(() => this.notifyChanged())
        );
    }

    delete(id: number) {
        return this.http.delete(`${this.apiUrl}/${id}`).pipe(
            tap(() => this.notifyChanged())
        );
    }
}
