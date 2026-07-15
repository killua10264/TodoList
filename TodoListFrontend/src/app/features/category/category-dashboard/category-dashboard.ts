import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TodoService } from '../../../core/services/todo.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryResponse } from '../../../core/models/category.model';
import { CategoryFormDialogComponent } from '../category-form-dialog/category-form-dialog';
import { TodoFormDialogComponent } from '../../todo/todo-form-dialog/todo-form-dialog';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';

@Component({
  selector: 'app-category-dashboard',
  imports: [
    RouterLink,
    CategoryFormDialogComponent,
    TodoFormDialogComponent,
    ConfirmDialogComponent,
    LoadingSpinnerComponent
  ],
  templateUrl: './category-dashboard.html',
  styleUrl: './category-dashboard.css'
})
export class CategoryDashboardComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private todoService = inject(TodoService);
  private toast = inject(ToastService);

  categories = signal<CategoryResponse[]>([]);
  isLoading = signal<boolean>(true);

  // Dialog Controls
  showCategoryForm = signal<boolean>(false);
  editingCategory = signal<CategoryResponse | null>(null);

  showTodoForm = signal<boolean>(false);
  selectedCategoryIdForTodo = signal<number | null>(null);

  showDeleteConfirm = signal<boolean>(false);
  deletingCategory = signal<CategoryResponse | null>(null);

  // Tổng hợp số liệu toàn khu vườn
  totalCategoriesCount = computed(() => this.categories().length);
  totalTodosCount = computed(() =>
    this.categories().reduce((sum, c) => sum + (c.todoCount ?? 0), 0)
  );
  totalCompletedCount = computed(() =>
    this.categories().reduce((sum, c) => sum + (c.completedTodoCount ?? 0), 0)
  );
  overallProgressPercent = computed(() => {
    const total = this.totalTodosCount();
    if (total === 0) return 0;
    return Math.round((this.totalCompletedCount() / total) * 100);
  });

  ngOnInit(): void {
    this.loadCategories();
    this.categoryService.refresh$.subscribe(() => this.loadCategories(true));
    this.todoService.refresh$.subscribe(() => this.loadCategories(true));
  }

  loadCategories(silent = false): void {
    if (!silent) {
      this.isLoading.set(true);
    }
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.show('Không thể tải dữ liệu khu vườn danh mục.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  getCategoryProgress(cat: CategoryResponse): number {
    const total = cat.todoCount ?? 0;
    if (total === 0) return 0;
    const completed = cat.completedTodoCount ?? 0;
    return Math.round((completed / total) * 100);
  }

  openCreateCategory(): void {
    this.editingCategory.set(null);
    this.showCategoryForm.set(true);
  }

  openEditCategory(cat: CategoryResponse): void {
    this.editingCategory.set(cat);
    this.showCategoryForm.set(true);
  }

  onCategoryFormSaved(): void {
    this.showCategoryForm.set(false);
    this.editingCategory.set(null);
    this.loadCategories();
  }

  openCreateTodoForCategory(categoryId: number): void {
    this.selectedCategoryIdForTodo.set(categoryId);
    this.showTodoForm.set(true);
  }

  onTodoFormSaved(): void {
    this.showTodoForm.set(false);
    this.selectedCategoryIdForTodo.set(null);
    this.loadCategories();
  }

  openDeleteCategoryConfirm(cat: CategoryResponse): void {
    this.deletingCategory.set(cat);
    this.showDeleteConfirm.set(true);
  }

  onDeleteConfirmed(confirmed: boolean): void {
    const target = this.deletingCategory();
    this.showDeleteConfirm.set(false);
    if (confirmed && target) {
      this.categoryService.delete(target.id).subscribe({
        next: () => {
          this.toast.show(`Đã nhổ bỏ danh mục "${target.name}". Các công việc bên trong đã được chuyển giao an toàn!`, 'success');
          this.loadCategories();
        },
        error: () => {
          this.toast.show('Không thể xóa danh mục này.', 'error');
        }
      });
    }
    this.deletingCategory.set(null);
  }
}
