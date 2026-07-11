import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategoryResponse } from '../../../core/models/category.model';

@Component({
    selector: 'app-todo-filter-bar',
    imports: [FormsModule],
    templateUrl: './todo-filter-bar.html',
    styleUrl: './todo-filter-bar.css'
})
export class TodoFilterBarComponent {
    categories = input<CategoryResponse[]>([]);
    filterChanged = output<any>();

    searchText = '';
    selectedPriority = 0;
    selectedCategory = 0;

    onFilterChange() {
        this.filterChanged.emit({
            search: this.searchText,
            priority: this.selectedPriority,
            categoryId: this.selectedCategory
        });
    }
}
