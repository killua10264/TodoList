import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface PasswordChecklist {
    hasMinLength: boolean;
    hasUpperCase: boolean;
    hasNumber: boolean;
    hasSpecialChar: boolean;
}

export function checkPasswordStatus(password: string | null | undefined): PasswordChecklist {
    const val = password || '';
    return {
        hasMinLength: val.length >= 8,
        hasUpperCase: /[A-Z]/.test(val),
        hasNumber: /\d/.test(val),
        hasSpecialChar: /[\W_]/.test(val)
    };
}

export function passwordRequirementsValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = control.value;
        if (!value || typeof value !== 'string') {
            return null; // required validator lo phần rỗng
        }
        const status = checkPasswordStatus(value);
        if (!status.hasMinLength || !status.hasUpperCase || !status.hasNumber || !status.hasSpecialChar) {
            return { passwordRequirements: status };
        }
        return null;
    };
}

