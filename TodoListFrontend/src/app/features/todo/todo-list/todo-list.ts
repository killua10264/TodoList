import { Component, inject, OnInit, signal, PLATFORM_ID, DestroyRef } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
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

import { LanguageService } from '../../../core/services/language.service';

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
  private destroyRef = inject(DestroyRef);
  langService = inject(LanguageService);

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
  searchText = signal<string>('');
  private searchSubject = new Subject<string>();

  showForm = false;
  editingTodo: TodoResponse | null = null;
  showCategoryForm = false;

  showDeleteConfirm = false;
  deletingTodoId: number | null = null;

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
        this.urlFilter.set(params['filter'] || null);
        const projId = (params['categoryId'] || params['projectId']) ? Number(params['categoryId'] || params['projectId']) : null;
        this.selectedCategoryId.set(projId);
        this.currentPage.set(1);
        this.loadTodos();
      });

      this.loadCategories();

      // Search debounce: đợi 400ms sau khi ngừng gõ mới gọi API
      this.searchSubject.pipe(
        debounceTime(400),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe(text => {
        this.searchText.set(text);
        this.currentPage.set(1);
        this.loadTodos();
      });

      this.todoService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        this.loadTodos(true);
      });
      this.categoryService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        this.loadCategories();
      });
      this.todoService.todoUpdated$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((updated) => {
        this.todos.update(list => {
          const exists = list.some(t => t.id === updated.id);
          if (updated.isHidden) {
             return list.filter(t => t.id !== updated.id);
          } else if (!exists) {
             return [updated, ...list];
          }
          return list.map(t => t.id === updated.id ? updated : t);
        });
      });
    }
  }

  onSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value);
  }

  openCreateCategoryForm() {
    this.showCategoryForm = true;
  }

  onCategoryFormSaved() {
    this.showCategoryForm = false;
  }

  loadTodos(silent = false) {
    if (!silent) {
      this.isLoading.set(true);
    }

    const search = this.searchText().trim() || null;
    const currentStatus = this.selectedStatus();
    
    let isHiddenParam: boolean | null = false;
    let isDeletedParam: boolean | null = false;
    let apiStatusParam: string | null = currentStatus;

    if (currentStatus === 'hidden') {
      isHiddenParam = true;
      isDeletedParam = false;
      apiStatusParam = 'all';
    } else if (currentStatus === 'trash') {
      isHiddenParam = null;
      isDeletedParam = true;
      apiStatusParam = 'all';
    }

    this.todoService.getAll(
      this.currentPage(),
      this.pageSize,
      this.urlFilter(),
      this.selectedCategoryId(),
      apiStatusParam,
      this.selectedSort(),
      isHiddenParam,
      search,
      isDeletedParam
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  onRestoreTodo(id: number) {
    this.todoService.restore(id).subscribe({
      next: () => {
        this.toast.show('Đã khôi phục công việc thành công! ♻️', 'success');
        this.loadTodos();
      },
      error: () => {
        this.toast.show('Không thể khôi phục công việc.', 'error');
      }
    });
  }

  onHardDeleteTodo(id: number) {
    if (confirm('Bạn có chắc chắn muốn xóa VĨNH VIỄN công việc này? Thao tác này không thể hoàn tác!')) {
      this.todoService.hardDelete(id).subscribe({
        next: () => {
          this.toast.show('Đã xóa vĩnh viễn công việc.', 'info');
          this.loadTodos();
        },
        error: () => {
          this.toast.show('Không thể xóa vĩnh viễn công việc.', 'error');
        }
      });
    }
  }

  onFilterChanged() {
    this.currentPage.set(1);
    this.loadTodos();
  }

  loadCategories() {
    this.categoryService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (projs) => this.categories.set(projs)
    });
  }

  onTodoToggled(todo: TodoResponse) {
    const newCompleted = !todo.isCompleted;

    // Optimistic: update UI immediately
    this.todos.update(list =>
      list.map(t => t.id === todo.id ? { ...t, isCompleted: newCompleted } : t)
    );

    const updateReq: TodoUpdateRequest = {
      title: todo.title,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
      categoryId: todo.categoryId,
      isCompleted: newCompleted
    };

    this.todoService.updateSilent(todo.id, updateReq).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (updated) => {
        this.todoService.todoUpdated$.next(updated);
      },
      error: () => {
        // Rollback on error
        this.todos.update(list =>
          list.map(t => t.id === todo.id ? { ...t, isCompleted: !newCompleted } : t)
        );
        this.toast.show('Cập nhật trạng thái thất bại.', 'error');
      }
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
  }

  onDeleteFromForm(todoId: number) {
    this.showForm = false;
    this.editingTodo = null;
    this.onDeleteRequested(todoId);
  }

  onDeleteRequested(todoId: number) {
    this.deletingTodoId = todoId;
    this.showDeleteConfirm = true;
  }

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm = false;
    if (confirmed && this.deletingTodoId) {
      const idToDelete = this.deletingTodoId;

      // Optimistic: remove from UI immediately
      const previousTodos = this.todos();
      const previousTotal = this.totalCount();
      this.todos.update(list => list.filter(t => t.id !== idToDelete));
      this.totalCount.update(c => Math.max(0, c - 1));

      this.todoService.delete(idToDelete).subscribe({
        next: () => {
          this.toast.show('Xóa công việc thành công!', 'success');
          if (previousTodos.length === 1 && this.currentPage() > 1) {
            this.currentPage.update(p => p - 1);
            this.loadTodos();
          }
        },
        error: () => {
          // Rollback on error
          this.todos.set(previousTodos);
          this.totalCount.set(previousTotal);
          this.toast.show('Xóa thất bại.', 'error');
        }
      });
    }
    this.deletingTodoId = null;
  }

  onPageChanged(page: number) {
    this.currentPage.set(page);
    this.loadTodos();
  }
} 
   