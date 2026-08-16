import { Component, input, output, computed, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CategoryResponse } from '../../../../core/models/category.model';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-main-sidebar',
  imports: [RouterLink, CommonModule],
  templateUrl: './main-sidebar.component.html',
  styleUrl: './main-sidebar.component.css'
})
export class MainSidebarComponent {
  pageTitle = input.required<string>();
  langService = inject(LanguageService);
  
  categories = input.required<CategoryResponse[]>();
  activeCategoryMenuId = input<number | null>(null);

  onCreateCategory = output<void>();
  onToggleCategoryMenu = output<{ event: Event, cat: CategoryResponse }>();
  onDeleteCategory = output<{ event: Event, cat: CategoryResponse }>();
  onEditCategory = output<{ event: Event, cat: CategoryResponse }>();

  vineVisible = computed(() => {
    const title = this.pageTitle();
    if (!title) return false;
    const allTasks = this.langService.translate('nav.all_tasks');
    const today = this.langService.translate('nav.today');
    const upcoming = this.langService.translate('nav.upcoming');
    const categories = this.langService.translate('nav.categories');
    const catDetail = this.langService.translate('cat.detail_title');

    if ([allTasks, today, upcoming, categories, catDetail, 'Tất cả công việc', 'Hôm nay', 'Sắp tới', 'Khu vườn Danh mục'].includes(title)) return true;
    return this.categories().some(c => c.name === title);
  });

  vineTop = computed(() => {
    const title = this.pageTitle();
    const allTasks = this.langService.translate('nav.all_tasks');
    const today = this.langService.translate('nav.today');
    const upcoming = this.langService.translate('nav.upcoming');
    const categories = this.langService.translate('nav.categories');

    const mainTabs = [allTasks, today, upcoming, categories];
    let index = mainTabs.indexOf(title);
    if (index === -1) {
      if (title === 'Tất cả công việc') index = 0;
      else if (title === 'Hôm nay') index = 1;
      else if (title === 'Sắp tới') index = 2;
      else if (title === 'Khu vườn Danh mục') index = 3;
    }
    if (index === -1) {
      const isCategoryPage = title === this.langService.translate('cat.detail_title') || this.categories().some(c => c.name === title);
      if (isCategoryPage) {
        index = 3;
      }
    }
    if (index === -1) return '0rem';
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

  handleEditCategory(event: Event, cat: CategoryResponse) {
    this.onEditCategory.emit({ event, cat });
  }
}
