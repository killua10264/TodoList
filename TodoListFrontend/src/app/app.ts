import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastNotificationComponent } from './shared/toast-notification/toast-notification';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastNotificationComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TodoListFrontend');
}
