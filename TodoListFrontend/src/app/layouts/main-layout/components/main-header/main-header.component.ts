import { Component, input, output, signal, HostListener, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-main-header',
  imports: [RouterLink, CommonModule],
  templateUrl: './main-header.component.html',
  styleUrl: './main-header.component.css'
})
export class MainHeaderComponent {
  langService = inject(LanguageService);

  pageTitle = input.required<string>();
  headerAvatarUrl = input<string | null>(null);
  headerAvatarInitials = input<string>('AVT');
  userBio = input<string>('');

  onLogout = output<void>();

  showUserMenu = signal<boolean>(false);

  toggleUserMenu(event: Event) {
    event.stopPropagation();
    this.showUserMenu.set(!this.showUserMenu());
  }

  closeUserMenu() {
    this.showUserMenu.set(false);
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.closeUserMenu();
  }

  handleLogout() {
    this.closeUserMenu();
    this.onLogout.emit();
  }
}
