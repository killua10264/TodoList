import { Component, inject, OnInit, signal, PLATFORM_ID, DestroyRef } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
  private destroyRef = inject(DestroyRef); // Đã bỏ Router vì không dùng đến

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
  // Đã xóa deletingCategoryId vì component này không chịu trách nhiệm xóa category

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

      // Chỉ lắng nghe sự kiện của Todo
      this.todoService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        this.loadTodos(true);
      });

      // Chỉ lắng nghe sự kiện của Category
      this.categoryService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        this.loadCategories();
      });
    }
  }

  // Đã bỏ goToCategoryDetail() vì việc filter danh mục đã được handle qua ngModel trên select

  openCreateCategoryForm() {
    this.showCategoryForm = true;
  }

  onCategoryFormSaved() {
    this.showCategoryForm = false;
    // Bỏ this.loadCategories() vì refresh$ đã lo
  }

  loadTodos(silent = false) {
    if (!silent) {
      this.isLoading.set(true);
    }
    // Bỏ takeUntilDestroyed dư thừa
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
    this.loadTodos(); // Đây là hành động manual từ UI nên vẫn giữ lại
  }

  loadCategories() {
    // Bỏ takeUntilDestroyed dư thừa
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

    // Bỏ takeUntilDestroyed dư thừa
    this.todoService.update(todo.id, updateReq).subscribe({
      next: () => {
        // Không cần cập nhật mảng thủ công nếu Service phát tín hiệu refresh$
        // Hàm loadTodos(true) sẽ tự động chạy và lấy state mới nhất
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
    // Bỏ this.loadTodos() vì refresh$ đã lo
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

  // Đã xóa hàm onDeleteCategoryRequested(event: Event, projId: number) vì dead code

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm = false;
    if (confirmed && this.deletingTodoId) {
      this.todoService.delete(this.deletingTodoId).subscribe({
        next: () => {
          this.toast.show('Xóa công việc thành công!', 'success');
          // Bỏ this.loadTodos() vì refresh$ đã lo

          // Logic tùy chọn: Xử lý lùi trang nếu xóa phần tử cuối cùng của trang hiện tại
          if (this.todos().length === 1 && this.currentPage() > 1) {
            this.currentPage.update(p => p - 1);
          }
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