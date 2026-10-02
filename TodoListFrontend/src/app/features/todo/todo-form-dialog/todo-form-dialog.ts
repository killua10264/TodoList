import {
  Component,
  input,
  output,
  inject,
  OnInit,
  OnChanges,
  SimpleChanges,
  AfterViewChecked,
  ElementRef,
  HostListener,
} from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { TodoService }     from '../../../core/services/todo.service';
import { ToastService }    from '../../../core/services/toast.service';
import { TodoResponse }    from '../../../core/models/todo.model';
import { CategoryResponse } from '../../../core/models/category.model';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-todo-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './todo-form-dialog.html',
  styleUrl: './todo-form-dialog.css',
})
export class TodoFormDialogComponent implements OnInit, AfterViewChecked {
  private el        = inject(ElementRef);
  langService       = inject(LanguageService);
  private todoService = inject(TodoService);
  private toast     = inject(ToastService);

  todo              = input<TodoResponse | null>(null);
  categories        = input<CategoryResponse[]>([]);
  initialCategoryId = input<number | null>(null);

  saved     = output<void>();
  cancelled = output<void>();
  deleted   = output<number>();

  isLoading = false;

  private openerEl: HTMLElement | null = null;
  private needsFocus = true;

  todoForm = new FormGroup({
    title:       new FormControl('',    { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    description: new FormControl('',    { nonNullable: true, validators: [Validators.maxLength(1000)] }),
    priority:    new FormControl(1,     { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(5)] }),
    dueDate:     new FormControl('',    { nonNullable: true, validators: [Validators.required] }),
    categoryId:  new FormControl(3,     { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
    isCompleted: new FormControl(false, { nonNullable: true }),
  });

  get isEditMode(): boolean {
    return this.todo() !== null;
  }

  ngOnInit(): void {
    // Lưu phần tử đã mở dialog để trả focus khi đóng
    this.openerEl = document.activeElement as HTMLElement;

    const t = this.todo();
    if (t) {
      this.todoForm.patchValue({
        title:       t.title,
        description: t.description,
        priority:    +t.priority,
        dueDate:     t.dueDate.substring(0, 10),
        categoryId:  t.categoryId,
        isCompleted: t.isCompleted,
      });
    } else {
      const now   = new Date();
      const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
      let defaultCatId = this.initialCategoryId() ?? 3;
      if (!this.initialCategoryId() && this.categories().length > 0) {
        const otherCat = this.categories().find(c => c.name.toLowerCase().includes('khác'));
        if (otherCat) defaultCatId = otherCat.id;
      }
      this.todoForm.patchValue({ priority: 2, dueDate: today, categoryId: defaultCatId });
    }
  }

  ngAfterViewChecked(): void {
    if (this.needsFocus) {
      const firstInput = this.el.nativeElement.querySelector('#todo-title') as HTMLElement | null;
      firstInput?.focus();
      this.needsFocus = false;
    }
  }

  @HostListener('keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.close();
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

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }

  close(): void {
    this.cancelled.emit();
    // Trả focus về opener
    setTimeout(() => this.openerEl?.focus(), 50);
  }

  onSubmit(): void {
    this.todoForm.markAllAsTouched();
    if (this.todoForm.invalid) return;

    this.isLoading = true;
    const formValue = this.todoForm.getRawValue();
    const todoData  = {
      title:       formValue.title,
      description: formValue.description,
      priority:    formValue.priority,
      dueDate:     formValue.dueDate,
      categoryId:  formValue.categoryId,
    };

    if (this.isEditMode) {
      this.todoService.update(this.todo()!.id, {
        ...todoData,
        isCompleted: formValue.isCompleted,
        version:     this.todo()!.version,
      }).subscribe({
        next:  () => { this.toast.show('Cập nhật thành công!', 'success'); this.saved.emit(); },
        error: (err) => { this.isLoading = false; this.toast.show(this.extractError(err) || 'Cập nhật thất bại.', 'error'); },
      });
    } else {
      this.todoService.create(todoData).subscribe({
        next:  () => { this.toast.show('Tạo mới thành công!', 'success'); this.saved.emit(); },
        error: (err) => { this.isLoading = false; this.toast.show(this.extractError(err) || 'Tạo mới thất bại.', 'error'); },
      });
    }
  }

  private extractError(err: unknown): string {
    if (!err || typeof err !== 'object') return '';
    const body = (err as { error?: unknown }).error;
    if (body && typeof body === 'object' && 'errors' in body) {
      const errors   = (body as { errors?: Record<string, unknown> }).errors;
      const firstKey = errors ? Object.keys(errors)[0] : undefined;
      const messages = firstKey ? errors?.[firstKey] : undefined;
      if (Array.isArray(messages) && messages.length > 0) return String(messages[0]);
    }
    if (body && typeof body === 'object' && 'message' in body) {
      return String((body as { message?: unknown }).message ?? '');
    }
    return typeof body === 'string' ? body : '';
  }

  onDelete(): void {
    if (this.todo()) this.deleted.emit(this.todo()!.id);
  }
}
