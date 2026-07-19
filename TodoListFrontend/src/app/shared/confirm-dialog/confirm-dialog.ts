import { Component, input, output } from '@angular/core';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.html',
    styleUrl: './confirm-dialog.css'
})
export class ConfirmDialogComponent {
        visible = input.required<boolean>();

        title = input<string>('⚠️ Xác nhận');

        message = input<string>('Bạn có chắc chắn muốn thực hiện hành động này?');

        confirmed = output<boolean>();

    onConfirm() {
        this.confirmed.emit(true);
    }

    onCancel() {
        this.confirmed.emit(false);
    }
}

