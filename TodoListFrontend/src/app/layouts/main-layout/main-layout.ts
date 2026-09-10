import { Component, inject, signal, computed, OnInit, HostListener, DestroyRef, effect } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { AuthFacade } from '../../core/services/auth.facade';
import { UserService } from '../../core/services/user.service';
import { CategoryService } from '../../core/services/category.service';
import { TodoService } from '../../core/services/todo.service';
import { CategoryResponse } from '../../core/models/category.model';
import { CategoryFormDialogComponent } from '../../features/category/category-form-dialog/category-form-dialog';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog';
import { MainHeaderComponent } from './components/main-header/main-header.component';
import { MainSidebarComponent } from './components/main-sidebar/main-sidebar.component';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { LanguageService } from '../../core/services/language.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, CategoryFormDialogComponent, ConfirmDialogComponent, MainHeaderComponent, MainSidebarComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent implements OnInit {
  private authFacade = inject(AuthFacade);
  private userService = inject(UserService);
  private categoryService = inject(CategoryService);
  private todoService = inject(TodoService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  langService = inject(LanguageService);

  showCategoryModal = false;
  showDeleteCategoryConfirm = false;
  activeCategoryMenuId: number | null = null;
  deletingCategory: CategoryResponse | null = null;
  editingCategory: CategoryResponse | null = null;
  categories = signal<CategoryResponse[]>([]);
  pageTitle = signal<string>('');
  headerAvatarUrl = computed(() => this.userService.currentUser()?.avatarUrl || null);
  headerAvatarInitials = computed(() => {
    const u = this.userService.currentUser();
    if (!u) return 'AVT';
    const name = u.displayName || u.username || 'A';
    return name.charAt(0).toUpperCase();
  });
  userBio = computed(() => {
    return this.userService.currentUser()?.bio || '';
  });
  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.updatePageTitle(this.router.url);
      this.loadCategories();
    });

    let firstRefresh = true;
    effect(() => {
      this.categoryService.refreshVersion();
      this.todoService.refreshVersion();
      if (firstRefresh) {
        firstRefresh = false;
        return;
      }
      this.loadCategories();
    });
  }

  ngOnInit() {
    this.userService.getProfile().subscribe();
    this.loadCategories();
    this.updatePageTitle(this.router.url);
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



  private updatePageTitle(url: string) {
    if (url.includes('/profile')) {
      this.pageTitle.set(this.langService.translate('nav.profile'));
      return;
    }
    if (url.includes('/change-password')) {
      this.pageTitle.set(this.langService.translate('nav.change_password'));
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
          this.pageTitle.set(this.langService.translate('cat.detail_title'));
        }
      } else {
        this.pageTitle.set(this.langService.translate('nav.categories'));
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
        this.pageTitle.set(this.langService.translate('cat.detail_title'));
      }
    } else if (url.includes('filter=today')) {
      this.pageTitle.set(this.langService.translate('nav.today'));
    } else if (url.includes('filter=upcoming')) {
      this.pageTitle.set(this.langService.translate('nav.upcoming'));
    } else if (url.includes('categoryId=') || url.includes('projectId=')) {
      const match = url.match(/categoryId=(\d+)/) || url.match(/projectId=(\d+)/);
      if (match) {
        const id = +match[1];
        const proj = this.categories().find(p => p.id === id);
        if (proj) {
          this.pageTitle.set(proj.name);
        } else if (id === 1) {
          this.pageTitle.set(this.langService.translate('cat.study'));
        } else if (id === 2) {
          this.pageTitle.set(this.langService.translate('cat.work'));
        } else if (id === 3) {
          this.pageTitle.set(this.langService.translate('cat.other'));
        } else {
          this.pageTitle.set(this.langService.translate('nav.all_tasks'));
        }
      } else {
        this.pageTitle.set(this.langService.translate('nav.all_tasks'));
      }
    } else {
      this.pageTitle.set(this.langService.translate('nav.all_tasks'));
    }
  }

  openCreateCategoryModal() {
    this.editingCategory = null;
    this.showCategoryModal = true;
  }

  onCategoryCreated() {
    this.showCategoryModal = false;
    this.editingCategory = null;
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

  onEditSidebarCategory(event: any) {
    const e = event.event as Event;
    e.stopPropagation();
    e.preventDefault();
    this.closeCategoryMenu();
    this.editingCategory = event.cat;
    this.showCategoryModal = true;
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
      const idToDelete = this.deletingCategory.id;

      // Optimistic: remove from UI immediately
      const previousCategories = this.categories();
      this.categories.update(list => list.filter(c => c.id !== idToDelete));

      this.categoryService.delete(idToDelete).subscribe({
        next: () => {
          this.todoService.notifyChanged();
        },
        error: () => {
          // Rollback on error
          this.categories.set(previousCategories);
        }
      });
    }
    this.deletingCategory = null;
  }

  onLogout() {
    this.authFacade.logout().subscribe();
  }
}


