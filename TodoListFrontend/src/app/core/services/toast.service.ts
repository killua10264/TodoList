import { Injectable, signal, computed } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastMessage {
  id:   number;
  text: string;
  type: ToastType;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<ToastMessage[]>([]);
  private nextId = 0;

  /** success và info → aria-live="polite" */
  readonly politeToasts = computed(() =>
    this.toasts().filter(t => t.type === 'success' || t.type === 'info')
  );

  /** error và warning → aria-live="assertive" */
  readonly assertiveToasts = computed(() =>
    this.toasts().filter(t => t.type === 'error' || t.type === 'warning')
  );

  show(text: string, type: ToastType = 'info', durationMs = 4000): void {
    const id = this.nextId++;
    this.toasts.update(list => [...list, { id, text, type }]);
    setTimeout(() => this.remove(id), durationMs);
  }

  remove(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}
