import { Component, inject } from '@angular/core';
import {
  FormGroup, FormControl, Validators,
  ReactiveFormsModule, AbstractControl, ValidationErrors
} from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { checkPasswordStatus, passwordRequirementsValidator } from '../../../core/validators/password.validator';

@Component({
  selector: 'app-change-password',
  imports: [ReactiveFormsModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePasswordComponent {
  private userService = inject(UserService);
  private toast = inject(ToastService);

  isLoading = false;
  errorMessage = '';
  serverErrors: { [key: string]: string } = {};

  getPasswordStatus() {
    return checkPasswordStatus(this.passwordForm.get('newPassword')?.value);
  }

  passwordForm = new FormGroup({
    oldPassword: new FormControl('', Validators.required),
    newPassword: new FormControl('', [Validators.required, passwordRequirementsValidator()]),
    confirmNewPassword: new FormControl('', Validators.required)
  }, {
    // Validator cấp form (kiểm tra cross-field)
    validators: this.passwordMatchValidator
  });

  /** Custom validator: kiểm tra mật khẩu mới và xác nhận có khớp không */
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPass = control.get('newPassword')?.value;
    const confirm = control.get('confirmNewPassword')?.value;
    if (newPass && confirm && newPass !== confirm) {
      return { passwordMismatch: true };
    }
    return null;
  }

  // Xóa lỗi khi người dùng gõ lại
  onFieldInput(field: string) {
    if (this.serverErrors[field]) {
      delete this.serverErrors[field];
    }
    if (this.errorMessage) {
      this.errorMessage = '';
    }
  }

  onSubmit() {
    this.errorMessage = '';
    this.serverErrors = {};
    this.passwordForm.markAllAsTouched();

    if (this.passwordForm.invalid) {
      return;
    }

    this.isLoading = true;
    const data = this.passwordForm.value as any;

    this.userService.changePassword(data).subscribe({
      next: () => {
        this.isLoading = false;
        this.toast.show('Đổi mật khẩu thành công!', 'success');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isLoading = false;
        const body = err.error;

        // 1. Nếu có ValidationProblemDetails (danh sách lỗi từ FluentValidation hoặc ASP.NET Core)
        if (body && body.errors && typeof body.errors === 'object') {
          for (const key of Object.keys(body.errors)) {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            const messages = body.errors[key];
            this.serverErrors[camelKey] = Array.isArray(messages) ? messages[0] : String(messages);
          }
        }

        // 2. Lấy câu thông báo lỗi rõ ràng từ máy chủ (ví dụ sai mật khẩu cũ, trùng mật khẩu...)
        const msg = body?.message || 'Không thể đổi mật khẩu. Vui lòng kiểm tra lại thông tin.';
        this.errorMessage = msg;

        // 3. Tự động gán thông báo lỗi vào đúng ô input tương ứng (Sai ở đâu báo ở đấy)
        const lowerMsg = msg.toLowerCase();
        if (lowerMsg.includes('hiện tại') || lowerMsg.includes('mật khẩu cũ')) {
          this.serverErrors['oldPassword'] = msg;
        } else if (lowerMsg.includes('trùng với mật khẩu hiện tại') || lowerMsg.includes('mật khẩu mới')) {
          this.serverErrors['newPassword'] = msg;
        } else if (lowerMsg.includes('xác nhận') || lowerMsg.includes('không khớp')) {
          this.serverErrors['confirmNewPassword'] = msg;
        }
      }
    });
  }
}
