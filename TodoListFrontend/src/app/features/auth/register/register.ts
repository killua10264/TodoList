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
                const message = err.error?.message || 'Đăng ký thất bại. Vui lòng thử lại.';
                this.toast.show(message, 'error');
            }
        });
    }
}
