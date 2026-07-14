import { Component, input, output, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryResponse } from '../../../core/models/category.model';

@Component({
  selector: 'app-category-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './category-form-dialog.html',
  styleUrl: './category-form-dialog.css'
})
export class CategoryFormDialogComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private toast = inject(ToastService);

  category = input<CategoryResponse | null>(null);

  saved = output<void>();
  cancelled = output<void>();
  deleted = output<number>();

  isLoading = false;

  categoryForm = new FormGroup({
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
    return this.category() !== null;
  }

  ngOnInit() {
    const p = this.category();
    if (p) {
      this.categoryForm.patchValue({
        name: p.name,
        color: p.color || '#4a5a3a'
      });
    } else {
      this.categoryForm.patchValue({
        name: '',
        color: '#4a5a3a'
      });
    }
  }

  selectColor(hex: string) {
    this.categoryForm.patchValue({ color: hex });
  }

  onSubmit() {
    this.categoryForm.markAllAsTouched();
    if (this.categoryForm.invalid) return;

    this.isLoading = true;
    const formValue = {
      name: this.categoryForm.value.name || '',
      color: this.categoryForm.value.color || '#4a5a3a'
    };

    if (this.isEditMode) {
      this.categoryService.update(this.category()!.id, formValue).subscribe({
        next: () => {
          this.toast.show('Cập nhật danh mục thành công!', 'success');
          this.saved.emit();
        },
        error: () => {
          this.isLoading = false;
          this.toast.show('Cập nhật danh mục thất bại.', 'error');
        }
      });
    } else {
      this.categoryService.create(formValue).subscribe({
        next: () => {
          this.toast.show('Tạo danh mục mới thành công!', 'success');
          this.saved.emit();
        },
        error: () => {
          this.isLoading = false;
          this.toast.show('Tạo danh mục thất bại.', 'error');
        }
      });
    }
  }

  onDelete() {
    if (this.category()) {
      this.deleted.emit(this.category()!.id);
    }
  }
}
