import { Component, inject } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-register',
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './register.html',
    styleUrl: './register.css'
})
export class RegisterComponent {
    private authService = inject(AuthService);
    private router = inject(Router);
    private toast = inject(ToastService);

    isLoading = false;
    showPassword = false;

    togglePassword() {
        this.showPassword = !this.showPassword;
    }

    registerForm = new FormGroup({
        username: new FormControl('', [Validators.required, Validators.minLength(3)]),
        email: new FormControl('', [Validators.required, Validators.email]),
        password: new FormControl('', [Validators.required, Validators.minLength(6)])
    });

    onSubmit() {
        this.registerForm.markAllAsTouched();
        if (this.registerForm.invalid) return;

        this.isLoading = true;
        const { username, email, password } = this.registerForm.value;

        this.authService.register({
            username: username!,
            email: email!,
            password: password!
        }).subscribe({
            next: () => {
                this.toast.show('Đăng ký thành công!', 'success');
                this.router.navigate(['/todos']);
            },
            error: (err) => {
                this.isLoading = false;
                let message = 'Đăng ký thất bại. Vui lòng thử lại.';
                
                if (err.status === 0) {
                    message = 'Không thể kết nối đến máy chủ Backend (http://localhost:5205). Vui lòng đảm bảo Backend đang chạy.';
                } else if (err.status === 429) {
                    message = 'Bạn đã bấm đăng ký quá nhiều lần. Vui lòng đợi 1 phút trước khi thử lại.';
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
