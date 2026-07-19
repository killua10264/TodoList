import { Component, input, output, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CategoryResponse } from '../../../../core/models/category.model';
import { TodoResponse } from '../../../../core/models/todo.model';

@Component({
  selector: 'app-main-sidebar',
  imports: [RouterLink, CommonModule],
  templateUrl: './main-sidebar.component.html',
  styleUrl: './main-sidebar.component.css'
})
export class MainSidebarComponent {
  pageTitle = input.required<string>();
  
  categories = input.required<CategoryResponse[]>();
  activeCategoryMenuId = input<number | null>(null);

  sidebarTodos = input.required<TodoResponse[]>();
  sidebarPage = input.required<number>();
  sidebarTotalPages = input.required<number>();

  onCreateCategory = output<void>();
  onToggleCategoryMenu = output<{ event: Event, cat: CategoryResponse }>();
  onDeleteCategory = output<{ event: Event, cat: CategoryResponse }>();
  onToggleTodo = output<{ event: Event, todo: TodoResponse }>();
  onPrevPage = output<void>();
  onNextPage = output<void>();

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

  handleCreateCategory() {
    this.onCreateCategory.emit();
  }

  handleToggleCategoryMenu(event: Event, cat: CategoryResponse) {
    this.onToggleCategoryMenu.emit({ event, cat });
  }

  handleDeleteCategory(event: Event, cat: CategoryResponse) {
    this.onDeleteCategory.emit({ event, cat });
  }

  handleToggleTodo(event: Event, todo: TodoResponse) {
    this.onToggleTodo.emit({ event, todo });
  }

  handlePrevPage() {
    this.onPrevPage.emit();
  }

  handleNextPage() {
    this.onNextPage.emit();
  }
}
