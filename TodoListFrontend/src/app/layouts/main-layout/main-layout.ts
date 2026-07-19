import { Component, inject, signal, computed, OnInit, HostListener, DestroyRef } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { CategoryService } from '../../core/services/category.service';
import { TodoService } from '../../core/services/todo.service';
import { CategoryResponse } from '../../core/models/category.model';
import { TodoResponse, PaginatedResponse } from '../../core/models/todo.model';
import { CategoryFormDialogComponent } from '../../features/category/category-form-dialog/category-form-dialog';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog';
import { MainHeaderComponent } from './components/main-header/main-header.component';
import { MainSidebarComponent } from './components/main-sidebar/main-sidebar.component';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, CategoryFormDialogComponent, ConfirmDialogComponent, MainHeaderComponent, MainSidebarComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private categoryService = inject(CategoryService);
  private todoService = inject(TodoService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  showCategoryModal = false;
  showDeleteCategoryConfirm = false;
  activeCategoryMenuId: number | null = null;
  deletingCategory: CategoryResponse | null = null;
  categories = signal<CategoryResponse[]>([]);
  pageTitle = signal<string>('Tất cả công việc');
  headerAvatarUrl = computed(() => this.userService.currentUser()?.avatarUrl || null);
  headerAvatarInitials = computed(() => {
    const u = this.userService.currentUser();
    if (!u) return 'AVT';
    const name = u.displayName || u.username || 'A';
    return name.charAt(0).toUpperCase();
  });
  sidebarTodos = signal<TodoResponse[]>([]);
  sidebarTotalCount = signal(0);
  sidebarPage = signal(1);
  sidebarPageSize = 10;

  sidebarTotalPages = computed(() =>
    Math.max(1, Math.ceil(this.sidebarTotalCount() / this.sidebarPageSize))
  );
  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.updatePageTitle(this.router.url);
      this.loadCategories();
      this.sidebarPage.set(1);
      this.loadSidebarTodos();
    });
  }

  ngOnInit() {
    this.userService.getProfile().subscribe();
    this.loadCategories();
    this.loadSidebarTodos();
    this.updatePageTitle(this.router.url);

    this.categoryService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadCategories();
      this.loadSidebarTodos();
    });
    this.todoService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadSidebarTodos();
      this.loadCategories();
    });
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories.set(data);
        this.updatePageTitle(this.router.url);
      },
      error: () => { }
    });
  }

    loadSidebarTodos() {
    const url = this.router.url;
    let urlFilter: string | null = null;
    let categoryId: number | null = null;

    if (url.includes('filter=today')) urlFilter = 'today';
    else if (url.includes('filter=upcoming')) urlFilter = 'upcoming';

    const projMatch = url.match(/categoryId=(\d+)/) || url.match(/projectId=(\d+)/);
    if (projMatch) categoryId = +projMatch[1];

    this.todoService.getAll(
      this.sidebarPage(), this.sidebarPageSize,
      urlFilter, categoryId, null, null
    ).subscribe({
      next: (res: PaginatedResponse<TodoResponse>) => {
        this.sidebarTodos.set(res.items);
        this.sidebarTotalCount.set(res.totalCount);
      },
      error: () => { }
    });
  }

  onToggleSidebarTodo(event: any) {
    const e = event.event as Event;
    const todo = event.todo as TodoResponse;
    e.stopPropagation();
    e.preventDefault();
    this.todoService.update(todo.id, {
      title: todo.title,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
      isCompleted: !todo.isCompleted,
      categoryId: todo.categoryId
    }).subscribe();
  }

  sidebarPrevPage() {
    if (this.sidebarPage() > 1) {
      this.sidebarPage.update(p => p - 1);
      this.loadSidebarTodos();
    }
  }

  sidebarNextPage() {
    if (this.sidebarPage() < this.sidebarTotalPages()) {
      this.sidebarPage.update(p => p + 1);
      this.loadSidebarTodos();
    }
  }

  private updatePageTitle(url: string) {
    if (url.includes('/profile')) {
      this.pageTitle.set('Hồ sơ cá nhân');
      return;
    }
    if (url.includes('/change-password')) {
      this.pageTitle.set('Đổi mật khẩu');
      return;
    }
    if (url.includes('/categories')) {
      const projMatch = url.match(/\/categories\/(\d+)/);
      if (projMatch) {
        const id = +projMatch[1];
        const proj = this.categories().find(p => p.id === id);
        if (proj) {
          this.pageTitle.set(proj.name);
        } else {
          this.pageTitle.set('Chi tiết danh mục');
        }
      } else {
        this.pageTitle.set('Khu vườn Danh mục');
      }
      return;
    }
    const projMatch = url.match(/\/projects\/(\d+)/);
    if (projMatch) {
      const id = +projMatch[1];
      const proj = this.categories().find(p => p.id === id);
      if (proj) {
        this.pageTitle.set(proj.name);
      } else {
        this.pageTitle.set('Chi tiết danh mục');
      }
    } else if (url.includes('filter=today')) {
      this.pageTitle.set('Hôm nay');
    } else if (url.includes('filter=upcoming')) {
      this.pageTitle.set('Sắp tới');
    } else if (url.includes('categoryId=') || url.includes('projectId=')) {
      const match = url.match(/categoryId=(\d+)/) || url.match(/projectId=(\d+)/);
      if (match) {
        const id = +match[1];
        const proj = this.categories().find(p => p.id === id);
        if (proj) {
          this.pageTitle.set(proj.name);
        } else if (id === 1) {
          this.pageTitle.set('Học tập');
        } else if (id === 2) {
          this.pageTitle.set('Công việc');
        } else if (id === 3) {
          this.pageTitle.set('Khác');
        } else {
          this.pageTitle.set('Tất cả công việc');
        }
      } else {
        this.pageTitle.set('Tất cả công việc');
      }
    } else {
      this.pageTitle.set('Tất cả công việc');
    }
  }

  openCreateCategoryModal() {
    this.showCategoryModal = true;
  }

  onCategoryCreated() {
    this.showCategoryModal = false;
    this.loadCategories();
  }

  @HostListener('document:click')
  onDocumentClick() {
    if (this.activeCategoryMenuId !== null) {
      this.activeCategoryMenuId = null;
    }
  }

  toggleCategoryMenu(event: any) {
    const e = event.event as Event;
    e.stopPropagation();
    e.preventDefault();
    if (this.activeCategoryMenuId === event.cat.id) {
      this.activeCategoryMenuId = null;
    } else {
      this.activeCategoryMenuId = event.cat.id;
    }
  }

  closeCategoryMenu() {
    this.activeCategoryMenuId = null;
  }

  onDeleteSidebarCategory(event: any) {
    const e = event.event as Event;
    e.stopPropagation();
    e.preventDefault();
    this.closeCategoryMenu();
    this.deletingCategory = event.cat;
    this.showDeleteCategoryConfirm = true;
  }

  onDeleteCategoryConfirmed(confirmed: boolean) {
    this.showDeleteCategoryConfirm = false;
    if (confirmed && this.deletingCategory) {
      this.categoryService.delete(this.deletingCategory.id).subscribe({
        next: () => {
          this.loadCategories();
          this.todoService.notifyChanged();
        },
        error: () => {}
      });
    }
    this.deletingCategory = null;
  }

  onLogout() {
    this.authService.logout().subscribe();
  }
}


