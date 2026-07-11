import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
    id: number;
    text: string;
    type: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
    // signal() = reactive state trong Angular 21
    toasts = signal<ToastMessage[]>([]);
    private nextId = 0;

    show(text: string, type: 'success' | 'error' | 'info' = 'info') {
        const id = this.nextId++;
        this.toasts.update(list => [...list, { id, text, type }]);

        setTimeout(() => this.remove(id), 3000);
    }

    remove(id: number) {
        this.toasts.update(list => list.filter(t => t.id !== id));
    }
}
