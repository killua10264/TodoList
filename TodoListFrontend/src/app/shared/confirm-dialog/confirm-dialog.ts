import { Component, input, output } from '@angular/core';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.html',
    styleUrl: './confirm-dialog.css'
})
export class ConfirmDialogComponent {
    /** Dialog có đang hiển thị không */
    visible = input.required<boolean>();

    /** Tiêu đề dialog */
    title = input<string>('⚠️ Xác nhận');

    /** Nội dung câu hỏi */
    message = input<string>('Bạn có chắc chắn muốn thực hiện hành động này?');

    /** Sự kiện khi người dùng chọn Xác nhận hoặc Hủy */
    confirmed = output<boolean>();

    onConfirm() {
        this.confirmed.emit(true);
    }

    onCancel() {
        this.confirmed.emit(false);
    }
}
