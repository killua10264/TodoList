import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CategoryService } from '../../core/services/category.service';
import { CategoryResponse } from '../../core/models/category.model';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  showUserMenu = false;
  categories = signal<CategoryResponse[]>([]);
  pageTitle = signal<string>('Tất cả công việc');

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updatePageTitle(this.router.url);
    });
  }

  ngOnInit() {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories.set(data),
      error: () => {}
    });
    this.updatePageTitle(this.router.url);
  }

  private updatePageTitle(url: string) {
    if (url.includes('/categories')) {
      this.pageTitle.set('Báo cáo');
    } else if (url.includes('filter=today')) {
      this.pageTitle.set('Hôm nay');
    } else if (url.includes('filter=upcoming')) {
      this.pageTitle.set('Sắp tới');
    } else if (url.includes('categoryId=1')) {
      this.pageTitle.set('Học tập');
    } else if (url.includes('categoryId=2')) {
      this.pageTitle.set('Công việc');
    } else if (url.includes('categoryId=3')) {
      this.pageTitle.set('Khác');
    } else {
      this.pageTitle.set('Tất cả công việc');
    }
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
