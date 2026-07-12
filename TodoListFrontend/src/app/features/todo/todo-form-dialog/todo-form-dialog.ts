import { Component, input, output, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { TodoService } from '../../../core/services/todo.service';
import { ToastService } from '../../../core/services/toast.service';
import { TodoResponse } from '../../../core/models/todo.model';
import { ProjectResponse } from '../../../core/models/project.model';

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
  projects = input<ProjectResponse[]>([]);

  saved = output<void>();
  cancelled = output<void>();
  deleted = output<number>();

  isLoading = false;

  todoForm = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl(''),
    priority: new FormControl(1, [Validators.required]),
    dueDate: new FormControl('', [Validators.required]),
    projectId: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    isCompleted: new FormControl(false)
  });

  get isEditMode(): boolean {
    return this.todo() !== null;
  }

  ngOnInit() {
    const t = this.todo();
    if (t) {
      let formProjId = +t.projectId;
      const projName = (t.projectName || '').replace('#', '').trim().toLowerCase();
      if (projName.includes('học tập')) formProjId = 1;
      else if (projName.includes('công việc')) formProjId = 2;
      else if (projName.includes('khác')) formProjId = 3;
      else if (formProjId !== 1 && formProjId !== 2 && formProjId !== 3) formProjId = 1;

      this.todoForm.patchValue({
        title: t.title,
        description: t.description,
        priority: +t.priority,
        dueDate: t.dueDate.substring(0, 10),
        projectId: formProjId,
        isCompleted: t.isCompleted
      });
    } else {
      const today = new Date().toISOString().substring(0, 10);
      this.todoForm.patchValue({
        priority: 2, // Trung bình
        dueDate: today,
        projectId: 1 // Mặc định là Học tập
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
      projectId: +(this.todoForm.value.projectId || 1)
    };

    if (this.isEditMode) {
      this.todoService.update(this.todo()!.id, formValue as any).subscribe({
        next: () => { this.toast.show('Cập nhật thành công!', 'success'); this.saved.emit(); },
        error: () => { this.isLoading = false; this.toast.show('Cập nhật thất bại.', 'error'); }
      });
    } else {
      this.todoService.create(formValue as any).subscribe({
        next: () => { this.toast.show('Tạo mới thành công!', 'success'); this.saved.emit(); },
        error: () => { this.isLoading = false; this.toast.show('Tạo mới thất bại.', 'error'); }
      });
    }
  }

  onDelete() {
    if (this.todo()) {
      this.deleted.emit(this.todo()!.id);
    }
  }
}
