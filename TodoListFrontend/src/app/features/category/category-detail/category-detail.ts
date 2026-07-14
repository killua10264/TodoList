import { Component, inject, OnInit, signal, PLATFORM_ID, computed } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { TodoService } from '../../../core/services/todo.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryResponse } from '../../../core/models/category.model';
import { TodoResponse, PaginatedResponse, TodoUpdateRequest } from '../../../core/models/todo.model';

import { TodoItemComponent } from '../../todo/todo-item/todo-item';
import { TodoFormDialogComponent } from '../../todo/todo-form-dialog/todo-form-dialog';
import { CategoryFormDialogComponent } from '../category-form-dialog/category-form-dialog';
import { PaginationComponent } from '../../../shared/pagination/pagination';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-category-detail',
  imports: [
    FormsModule,
    TodoItemComponent,
    TodoFormDialogComponent,
    CategoryFormDialogComponent,
    PaginationComponent,
    LoadingSpinnerComponent,
    ConfirmDialogComponent
  ],
  templateUrl: './category-detail.html',
  styleUrl: './category-detail.css'
})
export class CategoryDetailComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private todoService = inject(TodoService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  category = signal<CategoryResponse | null>(null);
  allCategories = signal<CategoryResponse[]>([]);
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

  showCategoryForm = false;

  showDeleteConfirm = false;
  deleteTargetType: 'todo' | 'category' = 'todo';
  deletingId: number | null = null;

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.route.paramMap.subscribe(params => {
        const id = Number(params.get('id'));
        if (id) {
          this.currentPage.set(1);
          this.loadCategoryDetail(id);
        }
      });
      this.loadAllCategories();
    }
  }

  loadAllCategories() {
    this.categoryService.getAll().subscribe({
      next: (projs) => this.allCategories.set(projs)
    });
  }

  loadCategoryDetail(id: number) {
    this.isLoading.set(true);
    this.categoryService.getById(id).subscribe({
      next: (proj) => {
        this.category.set(proj);
        this.loadCategoryTodos(id);
      },
      error: () => {
        this.toast.show('Không thể tải thông tin danh mục.', 'error');
        this.isLoading.set(false);
        this.router.navigate(['/todos']);
      }
    });
  }

  loadCategoryTodos(categoryId: number) {
    this.todoService.getAll(
      this.currentPage(),
      this.pageSize,
      null,
      categoryId,
      this.selectedStatus(),
      this.selectedSort()
    ).subscribe({
      next: (res: PaginatedResponse<TodoResponse>) => {
        this.todos.set(res.items);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.show('Không thể tải danh sách công việc của danh mục.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  onFilterChanged() {
    const p = this.category();
    if (p) {
      this.currentPage.set(1);
      this.loadCategoryTodos(p.id);
    }
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
    const p = this.category();
    if (p) {
      this.loadCategoryTodos(p.id);
    }
  }

  onTodoDeleteFromForm(todoId: number) {
    this.showTodoForm = false;
    this.editingTodo = null;
    this.openDeleteConfirm('todo', todoId);
  }

  openEditCategoryForm() {
    this.showCategoryForm = true;
  }

  onCategoryFormSaved() {
    this.showCategoryForm = false;
    const p = this.category();
    if (p) {
      this.loadCategoryDetail(p.id);
      this.loadAllCategories();
    }
  }

  openDeleteConfirm(type: 'todo' | 'category', id: number) {
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
            const p = this.category();
            if (p) this.loadCategoryTodos(p.id);
          },
          error: () => this.toast.show('Xóa công việc thất bại.', 'error')
        });
      } else if (this.deleteTargetType === 'category') {
        this.categoryService.delete(this.deletingId).subscribe({
          next: () => {
            this.toast.show('Xóa danh mục thành công!', 'success');
            this.router.navigate(['/todos']);
          },
          error: () => this.toast.show('Xóa danh mục thất bại.', 'error')
        });
      }
    }
    this.deletingId = null;
  }

  onPageChanged(page: number) {
    this.currentPage.set(page);
    const p = this.category();
    if (p) {
      this.loadCategoryTodos(p.id);
    }
  }
}
