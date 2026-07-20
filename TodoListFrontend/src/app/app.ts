import { Component, signal, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ToastNotificationComponent } from './shared/toast-notification/toast-notification';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastNotificationComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('TodoListFrontend');
  private themeService = inject(ThemeService);
  private platformId = inject(PLATFORM_ID);

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      const cachedTheme = localStorage.getItem('user_theme') || 'system';
      this.themeService.initTheme(cachedTheme);
    }
  }
}
