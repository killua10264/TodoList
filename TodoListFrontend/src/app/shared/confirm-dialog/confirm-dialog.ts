import {
  Component,
  input,
  output,
  OnChanges,
  SimpleChanges,
  ElementRef,
  inject,
  AfterViewChecked,
  HostListener,
} from '@angular/core';

/**
 * Dialog xác nhận dùng chung.
 * — role="dialog" + aria-modal
 * — Focus trap: khi mở, focus vào nút Hủy; Tab/Shift+Tab giữ trong dialog.
 * — Escape để đóng.
 * — Focus quay về phần tử đã mở dialog (opener) khi đóng.
 */
@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialogComponent implements OnChanges, AfterViewChecked {
  private el = inject(ElementRef);

  visible = input.required<boolean>();
  title   = input<string>('Xác nhận hành động');
  message = input<string>('Bạn có chắc chắn muốn thực hiện hành động này?');

  confirmed = output<boolean>();

  /** Phần tử đã mở dialog — để trả focus về khi đóng */
  private openerEl: HTMLElement | null = null;
  private needsFocus = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible']) {
      if (this.visible()) {
        this.openerEl = document.activeElement as HTMLElement;
        this.needsFocus = true;
      } else {
        // Trả focus về phần tử đã mở dialog
        this.openerEl?.focus();
        this.openerEl = null;
      }
    }
  }

  ngAfterViewChecked(): void {
    if (this.needsFocus && this.visible()) {
      const cancelBtn = this.el.nativeElement.querySelector('#dialog-cancel-btn') as HTMLElement | null;
      cancelBtn?.focus();
      this.needsFocus = false;
    }
  }

  @HostListener('keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (!this.visible()) return;

    if (event.key === 'Escape') {
      event.preventDefault();
      this.onCancel();
      return;
    }

    if (event.key === 'Tab') {
      this.trapFocus(event);
    }
  }

  private trapFocus(event: KeyboardEvent): void {
    const dialog = this.el.nativeElement.querySelector('[role="dialog"]') as HTMLElement;
    if (!dialog) return;

    const focusable = Array.from(
      dialog.querySelectorAll<HTMLElement>(
        'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
      )
    ).filter(el => el.offsetParent !== null);

    if (!focusable.length) return;

    const first = focusable[0];
    const last  = focusable[focusable.length - 1];

    if (event.shiftKey) {
      if (document.activeElement === first) {
        event.preventDefault();
        last.focus();
      }
    } else {
      if (document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    }
  }

  onConfirm(): void {
    this.confirmed.emit(true);
  }

  onCancel(): void {
    this.confirmed.emit(false);
  }
}
