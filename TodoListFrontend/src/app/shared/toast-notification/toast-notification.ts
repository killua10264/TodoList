import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
    selector: 'app-toast-notification',
    templateUrl: './toast-notification.html',
    styleUrl: './toast-notification.css'
})
export class ToastNotificationComponent {
    toastService = inject(ToastService);
}
