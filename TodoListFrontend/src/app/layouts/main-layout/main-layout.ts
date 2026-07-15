import { Component, inject, signal, computed, OnInit, HostListener } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { CategoryService } from '../../core/services/category.service';
import { TodoService } from '../../core/services/todo.service';
import { CategoryResponse } from '../../core/models/category.model';
import { TodoResponse, PaginatedResponse } from '../../core/models/todo.model';
import { CategoryFormDialogComponent } from '../../features/category/category-form-dialog/category-form-dialog';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, CategoryFormDialogComponent, ConfirmDialogComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private categoryService = inject(CategoryService);
  private todoService = inject(TodoService);
  private router = inject(Router);

  showUserMenu = false;
  showCategoryModal = false;
  showDeleteCategoryConfirm = false;
  activeCategoryMenuId: number | null = null;
  deletingCategory: CategoryResponse | null = null;
  categories = signal<CategoryResponse[]>([]);
  pageTitle = signal<string>('Tất cả công việc');

  // Dynamic Avatar từ currentUser signal của UserService
  headerAvatarUrl = computed(() => this.userService.currentUser()?.avatarUrl || null);
  headerAvatarInitials = computed(() => {
    const u = this.userService.currentUser();
    if (!u) return 'AVT';
    const name = u.displayName || u.username || 'A';
    return name.charAt(0).toUpperCase();
  });

  // Sidebar worklist: chỉ hiển thị tên các Todo card
  sidebarTodos = signal<TodoResponse[]>([]);
  sidebarTotalCount = signal(0);
  sidebarPage = signal(1);
  sidebarPageSize = 10;

  sidebarTotalPages = computed(() =>
    Math.max(1, Math.ceil(this.sidebarTotalCount() / this.sidebarPageSize))
  );

  private mainTabs = ['Tất cả công việc', 'Hôm nay', 'Sắp tới', 'Khu vườn Danh mục'];
  vineVisible = computed(() => {
    const title = this.pageTitle();
    return this.mainTabs.includes(title) || title === 'Báo cáo' || title === 'Danh mục';
  });
  vineTop = computed(() => {
    let index = this.mainTabs.indexOf(this.pageTitle());
    if (index === -1 && (this.pageTitle() === 'Báo cáo' || this.pageTitle() === 'Danh mục')) {
      index = 3;
    }
    if (index === -1) return '2.6rem';
    const itemHeight = 2.2;
    const gap = 1.5;
    const sidebarPadTop = 1;
    const top = sidebarPadTop + index * (itemHeight + gap) + itemHeight - 0.3;
    return top + 'rem';
  });

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
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

    this.categoryService.refresh$.subscribe(() => {
      this.loadCategories();
      this.loadSidebarTodos();
    });
    this.todoService.refresh$.subscribe(() => {
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

  /** Tải danh sách todo cho sidebar — đồng bộ filter với URL hiện tại */
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

  onToggleSidebarTodo(event: Event, todo: TodoResponse) {
    event.stopPropagation();
    event.preventDefault();
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

  toggleCategoryMenu(event: Event, cat: CategoryResponse) {
    event.stopPropagation();
    event.preventDefault();
    if (this.activeCategoryMenuId === cat.id) {
      this.activeCategoryMenuId = null;
    } else {
      this.activeCategoryMenuId = cat.id;
    }
  }

  closeCategoryMenu() {
    this.activeCategoryMenuId = null;
  }

  onDeleteSidebarCategory(event: Event, cat: CategoryResponse) {
    event.stopPropagation();
    event.preventDefault();
    this.closeCategoryMenu();
    this.deletingCategory = cat;
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

  toggleUserMenu() {
    this.showUserMenu = !this.showUserMenu;
  }

  closeUserMenu() {
    this.showUserMenu = false;
  }

  onLogout() {
    this.showUserMenu = false;
    this.authService.logout().subscribe();
  }
}
