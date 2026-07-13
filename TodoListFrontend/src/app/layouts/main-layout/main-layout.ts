import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProjectService } from '../../core/services/project.service';
import { TodoService } from '../../core/services/todo.service';
import { ProjectResponse } from '../../core/models/project.model';
import { TodoResponse, PaginatedResponse } from '../../core/models/todo.model';
import { ProjectFormDialogComponent } from '../../features/project/project-form-dialog/project-form-dialog';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, ProjectFormDialogComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private projectService = inject(ProjectService);
  private todoService = inject(TodoService);
  private router = inject(Router);

  showUserMenu = false;
  showProjectModal = false;
  projects = signal<ProjectResponse[]>([]);
  pageTitle = signal<string>('Tất cả công việc');

  // Sidebar worklist: chỉ hiển thị tên các Todo card
  sidebarTodos = signal<TodoResponse[]>([]);
  sidebarTotalCount = signal(0);
  sidebarPage = signal(1);
  sidebarPageSize = 10;

  sidebarTotalPages = computed(() =>
    Math.max(1, Math.ceil(this.sidebarTotalCount() / this.sidebarPageSize))
  );

  private mainTabs = ['Tất cả công việc', 'Hôm nay', 'Sắp tới', 'Báo cáo'];
  vineVisible = computed(() => this.mainTabs.includes(this.pageTitle()));
  vineTop = computed(() => {
    const index = this.mainTabs.indexOf(this.pageTitle());
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
      this.loadProjects();
      this.sidebarPage.set(1);
      this.loadSidebarTodos();
    });
  }

  ngOnInit() {
    this.loadProjects();
    this.loadSidebarTodos();
    this.updatePageTitle(this.router.url);

    this.projectService.refresh$.subscribe(() => this.loadProjects());
    this.todoService.refresh$.subscribe(() => {
      this.loadSidebarTodos();
    });
  }

  loadProjects() {
    this.projectService.getAll().subscribe({
      next: (data) => {
        this.projects.set(data);
        this.updatePageTitle(this.router.url);
      },
      error: () => { }
    });
  }

  /** Tải danh sách todo cho sidebar — đồng bộ filter với URL hiện tại */
  loadSidebarTodos() {
    const url = this.router.url;
    let urlFilter: string | null = null;
    let projectId: number | null = null;

    if (url.includes('filter=today')) urlFilter = 'today';
    else if (url.includes('filter=upcoming')) urlFilter = 'upcoming';

    const projMatch = url.match(/projectId=(\d+)/);
    if (projMatch) projectId = +projMatch[1];

    this.todoService.getAll(
      this.sidebarPage(), this.sidebarPageSize,
      urlFilter, projectId, null, null
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
      projectId: todo.projectId
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
    const projMatch = url.match(/\/projects\/(\d+)/);
    if (projMatch) {
      const id = +projMatch[1];
      const proj = this.projects().find(p => p.id === id);
      if (proj) {
        this.pageTitle.set(proj.name);
      } else {
        this.pageTitle.set('Chi tiết dự án');
      }
    } else if (url.includes('filter=today')) {
      this.pageTitle.set('Hôm nay');
    } else if (url.includes('filter=upcoming')) {
      this.pageTitle.set('Sắp tới');
    } else if (url.includes('projectId=')) {
      const match = url.match(/projectId=(\d+)/);
      if (match) {
        const id = +match[1];
        const proj = this.projects().find(p => p.id === id);
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

  openCreateProjectModal() {
    this.showProjectModal = true;
  }

  onProjectCreated() {
    this.showProjectModal = false;
    this.loadProjects();
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
