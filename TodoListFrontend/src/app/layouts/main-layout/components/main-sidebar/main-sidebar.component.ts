import { Component, input, output, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CategoryResponse } from '../../../../core/models/category.model';

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

  onCreateCategory = output<void>();
  onToggleCategoryMenu = output<{ event: Event, cat: CategoryResponse }>();
  onDeleteCategory = output<{ event: Event, cat: CategoryResponse }>();

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
}
