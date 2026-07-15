import { Component, inject, OnInit, signal, computed, DestroyRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TodoService } from '../../../core/services/todo.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryResponse } from '../../../core/models/category.model';
import { CategoryFormDialogComponent } from '../category-form-dialog/category-form-dialog';
import { TodoFormDialogComponent } from '../../todo/todo-form-dialog/todo-form-dialog';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// Mở rộng interface để chứa thêm % progress đã tính toán sẵn
interface CategoryViewMode extends CategoryResponse {
  progressPercent: number;
}

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
  private destroyRef = inject(DestroyRef);

  // Dùng interface mới để render UI
  categories = signal<CategoryViewMode[]>([]);
  isLoading = signal<boolean>(true);

  showCategoryForm = signal<boolean>(false);
  editingCategory = signal<CategoryResponse | null>(null);

  showTodoForm = signal<boolean>(false);
  selectedCategoryIdForTodo = signal<number | null>(null);

  showDeleteConfirm = signal<boolean>(false);
  deletingCategory = signal<CategoryResponse | null>(null);

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

    // Chỉ đăng ký refresh$ tại đây, tự động dọn dẹp khi Component hủy
    this.categoryService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadCategories(true));

    this.todoService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadCategories(true));
  }

  loadCategories(silent = false): void {
    if (!silent) {
      this.isLoading.set(true);
    }

    // Đã bỏ takeUntilDestroyed dư thừa ở đây
    this.categoryService.getAll().subscribe({
      next: (data) => {
        // Map dữ liệu để tính toán % sẵn 1 lần duy nhất, giải phóng template HTML
        const mappedData: CategoryViewMode[] = data.map(cat => {
          const total = cat.todoCount ?? 0;
          const completed = cat.completedTodoCount ?? 0;
          const progressPercent = total === 0 ? 0 : Math.round((completed / total) * 100);
          return { ...cat, progressPercent };
        });

        this.categories.set(mappedData);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.show('Không thể tải dữ liệu khu vườn danh mục.', 'error');
        this.isLoading.set(false);
      }
    });
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
    // Bỏ this.loadCategories() vì refresh$ đã lo việc này
  }

  openCreateTodoForCategory(categoryId: number): void {
    this.selectedCategoryIdForTodo.set(categoryId);
    this.showTodoForm.set(true);
  }

  onTodoFormSaved(): void {
    this.showTodoForm.set(false);
    this.selectedCategoryIdForTodo.set(null);
    // Bỏ this.loadCategories() vì refresh$ đã lo việc này
  }

  openDeleteCategoryConfirm(cat: CategoryResponse): void {
    this.deletingCategory.set(cat);
    this.showDeleteConfirm.set(true);
  }

  onDeleteConfirmed(confirmed: boolean): void {
    const target = this.deletingCategory();
    this.showDeleteConfirm.set(false);

    if (confirmed && target) {
      // Đã bỏ takeUntilDestroyed dư thừa ở đây
      this.categoryService.delete(target.id).subscribe({
        next: () => {
          this.toast.show(`Đã nhổ bỏ danh mục "${target.name}". Các công việc bên trong đã được chuyển giao an toàn!`, 'success');
          // Bỏ this.loadCategories() vì refresh$ đã lo việc này
        },
        error: () => {
          this.toast.show('Không thể xóa danh mục này.', 'error');
        }
      });
    }
    this.deletingCategory.set(null);
  }
}