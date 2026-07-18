import { Component, inject } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { finalize } from 'rxjs/operators';

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
        usernameOrEmail: new FormControl('', [Validators.required]),
        password: new FormControl('', [Validators.required, Validators.minLength(6)])
    });

    onSubmit() {
        this.loginForm.markAllAsTouched();

        if (this.loginForm.invalid) return;

        this.isLoading = true;

        const { usernameOrEmail, password } = this.loginForm.value;

        this.authService.login({ usernameOrEmail: usernameOrEmail!, password: password! })
            .pipe(finalize(() => {
                this.isLoading = false;
            }))
            .subscribe({
                next: () => {
                    this.toast.show('Đăng nhập thành công!', 'success');
                    this.router.navigate(['/todos']);
                },
                error: (err) => {
                    console.log('Chi tiết lỗi API:', err);
                    let message = 'Tên đăng nhập/Email hoặc mật khẩu không đúng.';
                    if (err.status === 0) {
                        message = 'Không thể kết nối đến máy chủ Backend. Vui lòng thử lại sau.';
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
