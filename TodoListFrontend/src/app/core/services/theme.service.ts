import { Injectable, signal, PLATFORM_ID, inject, OnDestroy } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type ThemeMode = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService implements OnDestroy {
  private platformId = inject(PLATFORM_ID);
  private mediaQueryList: MediaQueryList | null = null;
  
  // Trạng thái lưu trữ UI (Sáng, Tối, hoặc Hệ thống)
  currentTheme = signal<ThemeMode>('light');
  
  // Trạng thái thực tế đang áp dụng lên thẻ HTML (Sáng hoặc Tối)
  activeMode = signal<'light' | 'dark'>('light');

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.mediaQueryList = window.matchMedia('(prefers-color-scheme: dark)');
      this.mediaQueryList.addEventListener('change', this.handleSystemThemeChange);
    }
  }

  // Khởi tạo từ bộ nhớ tạm (Cache) để chống FOUC.
  initTheme() {
    if (!isPlatformBrowser(this.platformId)) return;

    const cachedTheme = localStorage.getItem('user_theme');
    const theme: ThemeMode = cachedTheme === 'dark' || cachedTheme === 'system'
      ? cachedTheme
      : 'light';
    this.setTheme(theme, false);
  }

  setTheme(theme: ThemeMode, saveToStorage = true) {
    this.currentTheme.set(theme);
    if (saveToStorage && isPlatformBrowser(this.platformId)) {
      localStorage.setItem('user_theme', theme);
    }
    this.applyTheme();
  }

  private handleSystemThemeChange = (_event: MediaQueryListEvent): void => {
    if (this.currentTheme() === 'system') {
      this.applyTheme();
    }
  };

  private applyTheme() {
    if (!isPlatformBrowser(this.platformId)) return;

    let modeToApply: 'light' | 'dark' = 'light';
    const theme = this.currentTheme();

    if (theme === 'dark') {
      modeToApply = 'dark';
    } else if (theme === 'system') {
      modeToApply = this.mediaQueryList?.matches ? 'dark' : 'light';
    } else {
      modeToApply = 'light';
    }

    this.activeMode.set(modeToApply);

    if (modeToApply === 'dark') {
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
  }

  ngOnDestroy(): void {
    this.mediaQueryList?.removeEventListener('change', this.handleSystemThemeChange);
  }
}
