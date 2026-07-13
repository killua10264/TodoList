import { Component, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserResponse } from '../../../core/models/user.model';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-user-profile',
  imports: [ReactiveFormsModule],
  templateUrl: './user-profile.html',
  styleUrl: './user-profile.css'
})
export class UserProfileComponent implements OnInit {
  private userService = inject(UserService);
  private toast = inject(ToastService);
  private platformId = inject(PLATFORM_ID);

  isLoading = false;
  isFetching = true;
  avatarUrl: string | null = null;
  errorMessage = '';

  // Danh sách Múi giờ phổ biến
  timezones = [
    { value: 'Asia/Ho_Chi_Minh', label: 'Asia/Ho_Chi_Minh (UTC+07:00) - Hà Nội, TP.HCM' },
    { value: 'UTC', label: 'UTC (Giờ phối hợp quốc tế - UTC+00:00)' },
    { value: 'Asia/Tokyo', label: 'Asia/Tokyo (UTC+09:00) - Tokyo, Seoul' },
    { value: 'Asia/Singapore', label: 'Asia/Singapore (UTC+08:00) - Singapore, Bắc Kinh' },
    { value: 'Asia/Bangkok', label: 'Asia/Bangkok (UTC+07:00) - Bangkok, Jakarta' },
    { value: 'America/New_York', label: 'America/New_York (UTC-05:00) - Washington, New York' },
    { value: 'America/Los_Angeles', label: 'America/Los_Angeles (UTC-08:00) - California, San Francisco' },
    { value: 'Europe/London', label: 'Europe/London (UTC+00:00) - London, Dublin' },
    { value: 'Europe/Paris', label: 'Europe/Paris (UTC+01:00) - Paris, Berlin, Rome' },
    { value: 'Australia/Sydney', label: 'Australia/Sydney (UTC+10:00) - Sydney, Melbourne' }
  ];

  profileForm = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.minLength(3)]),
    displayName: new FormControl('', [Validators.maxLength(100)]),
    email: new FormControl({ value: '', disabled: true }), // Read-only
    bio: new FormControl('', [Validators.maxLength(300)]),
    timezone: new FormControl('Asia/Ho_Chi_Minh'),
    theme: new FormControl('light'),
    language: new FormControl('vi'),
    firstDayOfWeek: new FormControl('Monday')
  });

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadProfile();
    } else {
      this.isFetching = false;
    }
  }

  loadProfile() {
    this.isFetching = true;
    this.userService.getProfile().subscribe({
      next: (user: UserResponse) => {
        this.isFetching = false;
        this.avatarUrl = user.avatarUrl || null;
        this.profileForm.patchValue({
          username: user.username,
          displayName: user.displayName || user.username,
          email: user.email,
          bio: user.bio || '',
          timezone: user.timezone || 'Asia/Ho_Chi_Minh',
          theme: user.theme || 'light',
          language: user.language || 'vi',
          firstDayOfWeek: user.firstDayOfWeek || 'Monday'
        });
      },
      error: (err) => {
        this.isFetching = false;
        this.toast.show('Không thể tải hồ sơ cá nhân.', 'error');
      }
    });
  }

  /** Lấy chữ cái đầu làm Avatar mặc định */
  getAvatarInitials(): string {
    const name = this.profileForm.get('displayName')?.value || this.profileForm.get('username')?.value || 'U';
    return name.charAt(0).toUpperCase();
  }

  selectedFile: File | null = null;

  /** Xử lý tải ảnh lên (FileReader để preview, đồng thời lưu file để upload lên Cloudinary) */
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      if (file.size > 5 * 1024 * 1024) {
        this.toast.show('Ảnh vượt quá 5MB. Vui lòng chọn ảnh nhỏ hơn!', 'error');
        return;
      }
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = (e) => {
        this.avatarUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  /** Dùng ảnh mặc định (xóa avatarUrl) */
  useDefaultAvatar() {
    this.avatarUrl = null;
    this.selectedFile = null;
  }

  setTheme(themeValue: string) {
    this.profileForm.get('theme')?.setValue(themeValue);
  }

  setFirstDayOfWeek(dayValue: string) {
    this.profileForm.get('firstDayOfWeek')?.setValue(dayValue);
  }

  onSubmit() {
    this.errorMessage = '';
    this.profileForm.markAllAsTouched();
    if (this.profileForm.invalid) return;

    this.isLoading = true;

    // Nếu người dùng có chọn file ảnh mới, upload qua backend Cloudinary trước
    if (this.selectedFile) {
      this.userService.uploadAvatar(this.selectedFile).subscribe({
        next: (uploadRes) => {
          this.avatarUrl = uploadRes.avatarUrl;
          this.selectedFile = null;
          this.sendUpdateProfileRequest();
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || 'Lỗi tải ảnh lên Cloudinary. Vui lòng thử lại!';
          this.toast.show(this.errorMessage, 'error');
        }
      });
    } else {
      this.sendUpdateProfileRequest();
    }
  }

  private sendUpdateProfileRequest() {
    const rawValue = this.profileForm.getRawValue();
    const payload = {
      username: rawValue.username || undefined,
      email: rawValue.email || undefined,
      displayName: rawValue.displayName || undefined,
      bio: rawValue.bio || undefined,
      avatarUrl: this.avatarUrl || undefined,
      timezone: rawValue.timezone || undefined,
      theme: rawValue.theme || undefined,
      language: rawValue.language || undefined,
      firstDayOfWeek: rawValue.firstDayOfWeek || undefined
    };

    this.userService.updateProfile(payload).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.toast.show(res?.message || 'Cập nhật hồ sơ thành công!', 'success');
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Không thể cập nhật hồ sơ. Vui lòng thử lại!';
        this.toast.show(this.errorMessage, 'error');
      }
    });
  }
}
