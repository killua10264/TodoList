import { Component, inject, OnInit, signal, PLATFORM_ID, computed } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { TodoService } from '../../../core/services/todo.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProjectResponse } from '../../../core/models/project.model';
import { TodoResponse, PaginatedResponse, TodoUpdateRequest } from '../../../core/models/todo.model';

import { TodoItemComponent } from '../../todo/todo-item/todo-item';
import { TodoFormDialogComponent } from '../../todo/todo-form-dialog/todo-form-dialog';
import { ProjectFormDialogComponent } from '../project-form-dialog/project-form-dialog';
import { PaginationComponent } from '../../../shared/pagination/pagination';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-project-detail',
  imports: [
    FormsModule,
    TodoItemComponent,
    TodoFormDialogComponent,
    ProjectFormDialogComponent,
    PaginationComponent,
    LoadingSpinnerComponent,
    ConfirmDialogComponent
  ],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.css'
})
export class ProjectDetailComponent implements OnInit {
  private projectService = inject(ProjectService);
  private todoService = inject(TodoService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  project = signal<ProjectResponse | null>(null);
  allProjects = signal<ProjectResponse[]>([]);
  todos = signal<TodoResponse[]>([]);
  isLoading = signal(true);

  currentPage = signal(1);
  totalCount = signal(0);
  pageSize = 20;

  selectedStatus = signal<string>('all');
  selectedSort = signal<string>('dueDate');

  // Computed stats
  completedCount = computed(() => this.todos().filter(t => t.isCompleted).length);
  progressPercent = computed(() => {
    const total = this.todos().length;
    if (total === 0) return 0;
    return Math.round((this.completedCount() / total) * 100);
  });

  // Dialog controls
  showTodoForm = false;
  editingTodo: TodoResponse | null = null;

  showProjectForm = false;

  showDeleteConfirm = false;
  deleteTargetType: 'todo' | 'project' = 'todo';
  deletingId: number | null = null;

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.route.paramMap.subscribe(params => {
        const id = Number(params.get('id'));
        if (id) {
          this.currentPage.set(1);
          this.loadProjectDetail(id);
        }
      });
      this.loadAllProjects();
    }
  }

  loadAllProjects() {
    this.projectService.getAll().subscribe({
      next: (projs) => this.allProjects.set(projs)
    });
  }

  loadProjectDetail(id: number) {
    this.isLoading.set(true);
    this.projectService.getById(id).subscribe({
      next: (proj) => {
        this.project.set(proj);
        this.loadProjectTodos(id);
      },
      error: () => {
        this.toast.show('Không thể tải thông tin dự án.', 'error');
        this.isLoading.set(false);
        this.router.navigate(['/todos']);
      }
    });
  }

  loadProjectTodos(projectId: number) {
    this.todoService.getAll(
      this.currentPage(),
      this.pageSize,
      null,
      projectId,
      this.selectedStatus(),
      this.selectedSort()
    ).subscribe({
      next: (res: PaginatedResponse<TodoResponse>) => {
        this.todos.set(res.items);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.show('Không thể tải danh sách công việc của dự án.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  onFilterChanged() {
    const p = this.project();
    if (p) {
      this.currentPage.set(1);
      this.loadProjectTodos(p.id);
    }
  }

  onTodoToggled(todo: TodoResponse) {
    const updateReq: TodoUpdateRequest = {
      title: todo.title,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
      projectId: todo.projectId,
      isCompleted: !todo.isCompleted
    };
    this.todoService.update(todo.id, updateReq).subscribe({
      next: (updated) => {
        this.todos.update(list => list.map(t => t.id === todo.id ? updated : t));
      },
      error: () => this.toast.show('Cập nhật trạng thái thất bại.', 'error')
    });
  }

  openCreateTodoForm() {
    this.editingTodo = null;
    this.showTodoForm = true;
  }

  openEditTodoForm(todo: TodoResponse) {
    this.editingTodo = todo;
    this.showTodoForm = true;
  }

  onTodoFormSaved() {
    this.showTodoForm = false;
    this.editingTodo = null;
    const p = this.project();
    if (p) {
      this.loadProjectTodos(p.id);
    }
  }

  onTodoDeleteFromForm(todoId: number) {
    this.showTodoForm = false;
    this.editingTodo = null;
    this.openDeleteConfirm('todo', todoId);
  }

  openEditProjectForm() {
    this.showProjectForm = true;
  }

  onProjectFormSaved() {
    this.showProjectForm = false;
    const p = this.project();
    if (p) {
      this.loadProjectDetail(p.id);
      this.loadAllProjects();
    }
  }

  openDeleteConfirm(type: 'todo' | 'project', id: number) {
    this.deleteTargetType = type;
    this.deletingId = id;
    this.showDeleteConfirm = true;
  }

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm = false;
    if (confirmed && this.deletingId) {
      if (this.deleteTargetType === 'todo') {
        this.todoService.delete(this.deletingId).subscribe({
          next: () => {
            this.toast.show('Xóa công việc thành công!', 'success');
            const p = this.project();
            if (p) this.loadProjectTodos(p.id);
          },
          error: () => this.toast.show('Xóa công việc thất bại.', 'error')
        });
      } else if (this.deleteTargetType === 'project') {
        this.projectService.delete(this.deletingId).subscribe({
          next: () => {
            this.toast.show('Xóa dự án thành công!', 'success');
            this.router.navigate(['/todos']);
          },
          error: () => this.toast.show('Xóa dự án thất bại.', 'error')
        });
      }
    }
    this.deletingId = null;
  }

  onPageChanged(page: number) {
    this.currentPage.set(page);
    const p = this.project();
    if (p) {
      this.loadProjectTodos(p.id);
    }
  }
}
