import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const RESERVED_USERNAMES = ['admin', 'root', 'system', 'moderator', 'support', 'staff', 'superuser', 'null', 'undefined'];

export function usernameValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = control.value;
        if (!value || typeof value !== 'string') {
            return null; // Let required validator handle empty
        }

        const trimmed = value.trim();

        // 1. Length (3 - 30)
        if (trimmed.length < 3 || trimmed.length > 30) {
            return { usernameLength: true };
        }

        // 2. Allowed characters (no spaces, no diacritics, only a-zA-Z0-9_.)
        if (!/^[a-zA-Z0-9_.]+$/.test(trimmed)) {
            return { usernameChars: true };
        }

        // 3. Prefix / Suffix rules (cannot start/end with _ or .)
        if (trimmed.startsWith('_') || trimmed.startsWith('.') || trimmed.endsWith('_') || trimmed.endsWith('.')) {
            return { usernameEdge: true };
        }

        // 4. Consecutive symbols (no .., __, ._, _.)
        if (trimmed.includes('..') || trimmed.includes('__') || trimmed.includes('._') || trimmed.includes('_.')) {
            return { usernameConsecutive: true };
        }

        // 5. Reserved words
        if (RESERVED_USERNAMES.includes(trimmed.toLowerCase())) {
            return { usernameReserved: true };
        }

        return null;
    };
}

/** Trả về thông báo lỗi chi tiết cho UI từ errors của FormControl */
export function getUsernameErrorMessage(errors: ValidationErrors | null | undefined): string {
    if (!errors) return '';
    if (errors['required']) return 'Tên đăng nhập không được để trống.';
    if (errors['usernameLength']) return 'Tên đăng nhập phải có từ 3 đến 30 ký tự.';
    if (errors['usernameChars']) return 'Chỉ cho phép chữ cái không dấu (a-z), chữ số (0-9), dấu chấm (.) và dấu gạch dưới (_). Tuyệt đối cấm khoảng trắng và ký tự đặc biệt.';
    if (errors['usernameEdge']) return 'Không được bắt đầu hoặc kết thúc bằng dấu chấm (.) hoặc dấu gạch dưới (_).';
    if (errors['usernameConsecutive']) return 'Không được chứa 2 dấu chấm hoặc dấu gạch dưới nằm liền nhau.';
    if (errors['usernameReserved']) return 'Tên đăng nhập này chứa từ khóa nhạy cảm không được phép sử dụng.';
    return 'Tên đăng nhập không hợp lệ.';
}
