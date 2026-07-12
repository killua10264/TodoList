import { Component, input, output, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProjectResponse } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './project-form-dialog.html',
  styleUrl: './project-form-dialog.css'
})
export class ProjectFormDialogComponent implements OnInit {
  private projectService = inject(ProjectService);
  private toast = inject(ToastService);

  project = input<ProjectResponse | null>(null);

  saved = output<void>();
  cancelled = output<void>();
  deleted = output<number>();

  isLoading = false;

  projectForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    color: new FormControl('#4a5a3a', [Validators.required, Validators.maxLength(30)])
  });

  presetColors = [
    { label: 'Rêu Zen', value: '#4a5a3a' },
    { label: 'Xanh lục xám', value: '#6b7a45' },
    { label: 'Trà sẫm', value: '#8a8570' },
    { label: 'Hổ phách nhạt', value: '#b58d3d' },
    { label: 'Đất nung', value: '#a8583b' },
    { label: 'Lam đêm', value: '#3b586b' },
    { label: 'Tím mộng', value: '#6b457a' },
    { label: 'Đen than', value: '#333333' }
  ];

  get isEditMode(): boolean {
    return this.project() !== null;
  }

  ngOnInit() {
    const p = this.project();
    if (p) {
      this.projectForm.patchValue({
        name: p.name,
        color: p.color || '#4a5a3a'
      });
    } else {
      this.projectForm.patchValue({
        name: '',
        color: '#4a5a3a'
      });
    }
  }

  selectColor(hex: string) {
    this.projectForm.patchValue({ color: hex });
  }

  onSubmit() {
    this.projectForm.markAllAsTouched();
    if (this.projectForm.invalid) return;

    this.isLoading = true;
    const formValue = {
      name: this.projectForm.value.name || '',
      color: this.projectForm.value.color || '#4a5a3a'
    };

    if (this.isEditMode) {
      this.projectService.update(this.project()!.id, formValue).subscribe({
        next: () => {
          this.toast.show('Cập nhật dự án thành công!', 'success');
          this.saved.emit();
        },
        error: () => {
          this.isLoading = false;
          this.toast.show('Cập nhật dự án thất bại.', 'error');
        }
      });
    } else {
      this.projectService.create(formValue).subscribe({
        next: () => {
          this.toast.show('Tạo dự án mới thành công!', 'success');
          this.saved.emit();
        },
        error: () => {
          this.isLoading = false;
          this.toast.show('Tạo dự án thất bại.', 'error');
        }
      });
    }
  }

  onDelete() {
    if (this.project()) {
      this.deleted.emit(this.project()!.id);
    }
  }
}
