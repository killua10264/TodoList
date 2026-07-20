import { Component, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';

@Component({
    selector: 'app-home',
    templateUrl: './home.html',
    styleUrl: './home.css'
})
export class HomeComponent {
  themeService = inject(ThemeService);
}
