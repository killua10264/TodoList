import { Component, input, output, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { TodoService } from '../../../core/services/todo.service';
import { ToastService } from '../../../core/services/toast.service';
import { TodoResponse } from '../../../core/models/todo.model';
import { CategoryResponse } from '../../../core/models/category.model';

@Component({
  selector: 'app-todo-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './todo-form-dialog.html',
  styleUrl: './todo-form-dialog.css'
})
export class TodoFormDialogComponent implements OnInit {
  private todoService = inject(TodoService);
  private toast = inject(ToastService);

  todo = input<TodoResponse | null>(null);
  categories = input<CategoryResponse[]>([]);
  initialCategoryId = input<number | null>(null);

  saved = output<void>();
  cancelled = output<void>();
  deleted = output<number>();

  isLoading = false;

  todoForm = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl(''),
    priority: new FormControl(1, [Validators.required]),
    dueDate: new FormControl('', [Validators.required]),
    categoryId: new FormControl<number>(3, [Validators.required, Validators.min(1)]),
    isCompleted: new FormControl(false)
  });

  get isEditMode(): boolean {
    return this.todo() !== null;
  }

  ngOnInit() {
    const t = this.todo();
    if (t) {
      this.todoForm.patchValue({
        title: t.title,
        description: t.description,
        priority: +t.priority,
        dueDate: t.dueDate.substring(0, 10),
        categoryId: t.categoryId,
        isCompleted: t.isCompleted
      });
    } else {
      const today = new Date().toISOString().substring(0, 10);
      let defaultCatId = this.initialCategoryId() || 3;
      if (!this.initialCategoryId() && this.categories().length > 0) {
        const otherCat = this.categories().find(c => c.name.toLowerCase().includes('khác'));
        if (otherCat) defaultCatId = otherCat.id;
      }
      this.todoForm.patchValue({
        priority: 2,
        dueDate: today,
        categoryId: defaultCatId
      });
    }
  }

  onSubmit() {
    this.todoForm.markAllAsTouched();
    if (this.todoForm.invalid) return;

    this.isLoading = true;
    const formValue = {
      ...this.todoForm.value,
      priority: +(this.todoForm.value.priority || 1),
      categoryId: +(this.todoForm.value.categoryId || 1)
    };

    if (this.isEditMode) {
      this.todoService.update(this.todo()!.id, formValue as any).subscribe({
        next: () => { this.toast.show('Cập nhật thành công!', 'success'); this.saved.emit(); },
        error: (err) => { this.isLoading = false; this.toast.show(this.extractError(err) || 'Cập nhật thất bại.', 'error'); }
      });
    } else {
      this.todoService.create(formValue as any).subscribe({
        next: () => { this.toast.show('Tạo mới thành công!', 'success'); this.saved.emit(); },
        error: (err) => { this.isLoading = false; this.toast.show(this.extractError(err) || 'Tạo mới thất bại.', 'error'); }
      });
    }
  }

  private extractError(err: any): string {
    if (err.error?.errors) {
      const firstKey = Object.keys(err.error.errors)[0];
      return err.error.errors[firstKey][0];
    }
    return err.error?.message || err.error || '';
  }

  onDelete() {
    if (this.todo()) {
      this.deleted.emit(this.todo()!.id);
    }
  }
}
  
