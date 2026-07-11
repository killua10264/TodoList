import { Component, inject } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-login',
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './login.html',
    styleUrl: './login.css'
})
export class LoginComponent {
    private authService = inject(AuthService);
    private router = inject(Router);
    private toast = inject(ToastService);

    isLoading = false;
    showPassword = false;

    togglePassword() {
        this.showPassword = !this.showPassword;
    }

    loginForm = new FormGroup({
        email: new FormControl('', [Validators.required, Validators.email]),
        password: new FormControl('', [Validators.required, Validators.minLength(6)])
    });

    onSubmit() {
        this.loginForm.markAllAsTouched();

        if (this.loginForm.invalid) return;

        this.isLoading = true;

        const { email, password } = this.loginForm.value;

        this.authService.login({ email: email!, password: password! }).subscribe({
            next: () => {
                this.toast.show('Đăng nhập thành công!', 'success');
                this.router.navigate(['/todos']);
            },
            error: (err) => {
                this.isLoading = false;
                let message = 'Email hoặc mật khẩu không đúng.';
                if (err.status === 0) {
                    message = 'Không thể kết nối đến máy chủ Backend (http://localhost:5205). Vui lòng đảm bảo Backend đang chạy.';
                } else if (err.status === 429) {
                    message = 'Bạn đã thử đăng nhập quá nhiều lần. Vui lòng đợi 1 phút trước khi thử lại.';
                } else if (err.error?.errors) {
                    const firstErrorKey = Object.keys(err.error.errors)[0];
                    if (firstErrorKey && err.error.errors[firstErrorKey]?.length > 0) {
                        message = err.error.errors[firstErrorKey][0];
                    }
                } else if (err.error?.message) {
                    message = err.error.message;
                } else if (typeof err.error === 'string' && err.error.trim().length > 0) {
                    message = err.error;
                }
                this.toast.show(message, 'error');
            }
        });
    }
}
