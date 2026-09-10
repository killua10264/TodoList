import { Component, signal, OnInit, inject } from '@angular/core';
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

  ngOnInit() {
    this.themeService.initTheme();
  }
}
