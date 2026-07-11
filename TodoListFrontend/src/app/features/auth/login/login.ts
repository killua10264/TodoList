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
                const message = err.error?.message || 'Email hoặc mật khẩu không đúng.';
                this.toast.show(message, 'error');
            }
        });
    }
}
