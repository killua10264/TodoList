import { Component, inject } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-user-profile',
  imports: [ReactiveFormsModule],
  templateUrl: './user-profile.html',
  styleUrl: './user-profile.css'
})
export class UserProfileComponent {
  private userService = inject(UserService);
  private toast = inject(ToastService);

  isLoading = false;

  profileForm = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.minLength(3)]),
    email: new FormControl('', [Validators.required, Validators.email])
  });

  onSubmit() {
    this.profileForm.markAllAsTouched();
    if (this.profileForm.invalid) return;

    this.isLoading = true;
    const data = this.profileForm.value as any;

    this.userService.updateProfile(data).subscribe({
      next: () => {
        this.isLoading = false;
        this.toast.show('Cập nhật hồ sơ thành công!', 'success');
      },
      error: (err) => {
        this.isLoading = false;
        this.toast.show(err.error?.message || 'Cập nhật thất bại.', 'error');
      }
    });
  }
}
