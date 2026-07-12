import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProjectResponse } from '../../../core/models/project.model';

@Component({
    selector: 'app-todo-filter-bar',
    imports: [FormsModule],
    templateUrl: './todo-filter-bar.html',
    styleUrl: './todo-filter-bar.css'
})
export class TodoFilterBarComponent {
    projects = input<ProjectResponse[]>([]);
    filterChanged = output<any>();

    searchText = '';
    selectedPriority = 0;
    selectedProject = 0;

    onFilterChange() {
        this.filterChanged.emit({
            search: this.searchText,
            priority: this.selectedPriority,
            projectId: this.selectedProject
        });
    }
}
