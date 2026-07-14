import { Component, inject, OnInit, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TodoService } from '../../../core/services/todo.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { TodoResponse, PaginatedResponse, TodoUpdateRequest } from '../../../core/models/todo.model';
import { CategoryResponse } from '../../../core/models/category.model';

import { TodoItemComponent } from '../todo-item/todo-item';
import { TodoFormDialogComponent } from '../todo-form-dialog/todo-form-dialog';
import { CategoryFormDialogComponent } from '../../category/category-form-dialog/category-form-dialog';
import { PaginationComponent } from '../../../shared/pagination/pagination';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-todo-list',
  imports: [
    FormsModule,
    TodoItemComponent, TodoFormDialogComponent, CategoryFormDialogComponent,
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
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  todos = signal<TodoResponse[]>([]);
  categories = signal<CategoryResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  totalCount = signal(0);
  pageSize = 20;

  urlFilter = signal<string | null>(null);
  selectedCategoryId = signal<number | null>(null);
  selectedStatus = signal<string>('all');
  selectedSort = signal<string>('dueDate');

  showForm = false;
  editingTodo: TodoResponse | null = null;
  showCategoryForm = false;

  showDeleteConfirm = false;
  deletingTodoId: number | null = null;
  deletingCategoryId: number | null = null;

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.route.queryParams.subscribe(params => {
        this.urlFilter.set(params['filter'] || null);
        const projId = (params['categoryId'] || params['projectId']) ? Number(params['categoryId'] || params['projectId']) : null;
        this.selectedCategoryId.set(projId);
        this.currentPage.set(1);
        this.loadTodos();
      });
      this.loadCategories();
    }
  }

  goToCategoryDetail(projId: number) {
    this.selectedCategoryId.set(projId);
    this.onFilterChanged();
  }

  openCreateCategoryForm() {
    this.showCategoryForm = true;
  }

  onCategoryFormSaved() {
    this.showCategoryForm = false;
    this.loadCategories();
  }

  loadTodos() {
    this.isLoading.set(true);
    this.todoService.getAll(
      this.currentPage(),
      this.pageSize,
      this.urlFilter(),
      this.selectedCategoryId(),
      this.selectedStatus(),
      this.selectedSort()
    ).subscribe({
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

  onFilterChanged() {
    this.currentPage.set(1);
    this.loadTodos();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (projs) => this.categories.set(projs)
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

  onDeleteFromForm(todoId: number) {
    this.showForm = false;
    this.editingTodo = null;
    this.onDeleteRequested(todoId);
  }

  onDeleteRequested(todoId: number) {
    this.deletingTodoId = todoId;
    this.deletingCategoryId = null;
    this.showDeleteConfirm = true;
  }

  onDeleteCategoryRequested(event: Event, projId: number) {
    event.stopPropagation();
    this.deletingCategoryId = projId;
    this.deletingTodoId = null;
    this.showDeleteConfirm = true;
  }

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm = false;
    if (confirmed) {
      if (this.deletingTodoId) {
        this.todoService.delete(this.deletingTodoId).subscribe({
          next: () => {
            this.toast.show('Xóa công việc thành công!', 'success');
            this.loadTodos();
          },
          error: () => this.toast.show('Xóa thất bại.', 'error')
        });
      } else if (this.deletingCategoryId) {
        this.categoryService.delete(this.deletingCategoryId).subscribe({
          next: () => {
            this.toast.show('Xóa danh mục thành công!', 'success');
            this.loadCategories();
          },
          error: () => this.toast.show('Xóa danh mục thất bại.', 'error')
        });
      }
    }
    this.deletingTodoId = null;
    this.deletingCategoryId = null;
  }

  onPageChanged(page: number) {
    this.currentPage.set(page);
    this.loadTodos();
  }
}
