import { Component, input, output, computed } from '@angular/core';

@Component({
    selector: 'app-pagination',
    templateUrl: './pagination.html',
    styleUrl: './pagination.css'
})
export class PaginationComponent {
    currentPage = input.required<number>();
    totalCount = input.required<number>();
    pageSize = input<number>(20);

    // computed() = giá trị tự động tính lại khi input thay đổi
    totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));

    pageChanged = output<number>();

    goToPage(page: number) {
        if (page >= 1 && page <= this.totalPages()) {
            this.pageChanged.emit(page);
        }
    }
}
