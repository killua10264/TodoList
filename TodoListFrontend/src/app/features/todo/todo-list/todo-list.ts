import { Component, inject, OnInit, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { TodoService } from '../../../core/services/todo.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { TodoResponse, PaginatedResponse, TodoUpdateRequest } from '../../../core/models/todo.model';
import { CategoryResponse } from '../../../core/models/category.model';

import { TodoItemComponent } from '../todo-item/todo-item';
import { TodoFormDialogComponent } from '../todo-form-dialog/todo-form-dialog';
import { PaginationComponent } from '../../../shared/pagination/pagination';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-todo-list',
  imports: [
    TodoItemComponent, TodoFormDialogComponent,
    PaginationComponent, LoadingSpinnerComponent, ConfirmDialogComponent
  ],
  templateUrl: './todo-list.html',
  styleUrl: './todo-list.css'
})
export class TodoListComponent implements OnInit {
  private todoService = inject(TodoService);
  private categoryService = inject(CategoryService);
  private toast = inject(ToastService);
  private platformId = inject(PLATFORM_ID);

  todos = signal<TodoResponse[]>([]);
  categories = signal<CategoryResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  totalCount = signal(0);
  pageSize = 20;

  showForm = false;
  editingTodo: TodoResponse | null = null;

  showDeleteConfirm = false;
  deletingTodoId: number | null = null;

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadTodos();
      this.loadCategories();
    }
  }

  loadTodos() {
    this.isLoading.set(true);
    this.todoService.getAll(this.currentPage(), this.pageSize).subscribe({
      next: (res: PaginatedResponse<TodoResponse>) => {
        this.todos.set(res.items);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.show('Không thể tải danh sách công việc.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories.set(cats)
    });
  }

  onTodoToggled(todo: TodoResponse) {
    const updateReq: TodoUpdateRequest = {
      title: todo.title,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
      categoryId: todo.categoryId,
      isCompleted: !todo.isCompleted
    };
    this.todoService.update(todo.id, updateReq).subscribe({
      next: (updated) => {
        this.todos.update(list => list.map(t => t.id === todo.id ? updated : t));
      },
      error: () => this.toast.show('Cập nhật trạng thái thất bại.', 'error')
    });
  }

  openCreateForm() {
    this.editingTodo = null;
    this.showForm = true;
  }

  openEditForm(todo: TodoResponse) {
    this.editingTodo = todo;
    this.showForm = true;
  }

  onFormSaved() {
    this.showForm = false;
    this.editingTodo = null;
    this.loadTodos();
  }

  onDeleteRequested(todoId: number) {
    this.deletingTodoId = todoId;
    this.showDeleteConfirm = true;
  }

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm = false;
    if (confirmed && this.deletingTodoId) {
      this.todoService.delete(this.deletingTodoId).subscribe({
        next: () => {
          this.toast.show('Xóa thành công!', 'success');
          this.loadTodos();
        },
        error: () => this.toast.show('Xóa thất bại.', 'error')
      });
    }
    this.deletingTodoId = null;
  }

  onPageChanged(page: number) {
    this.currentPage.set(page);
    this.loadTodos();
  }
}
